using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class BimsService
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly CommonRequestsUtility commonRequests;
        private readonly ILogger<BimsService> logger;

        public BimsService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests,
            ILogger<BimsService> logger)
        {
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
            this.logger = logger;
            logger.LogTrace("BimsService created");
        }

        public async Task<IEnumerable<ObjectBase>> GetAllBimFilesAsync()
        {
            logger.LogTrace("GetAllBimFilesAsync started");
            return await commonRequests.RetrieveObjectsListAsync(OBJECTTYPE_BIM);
        }

        public async Task<List<BimAttribute>> GetBimAttributesAsync(string bimId, string folderId)
        {
            logger.LogTrace(
                "GetBimAttributesAsync started with bimId: {@BimID}, folderId: {@FolderID}",
                bimId,
                folderId);
            var data = new
            {
                folderId = folderId,
                objectId = bimId,
            };

            var response = await requestUtility.GetResponseAsync(Resources.MethodBimGetListViewObjectAttributes, data);
            return response.ToObject<List<BimAttribute>>();
        }

        public async Task<List<ObjectBase>> GetBimsChildObjectsByAttributeAsync(string bimId, string folderKey, string attributeId)
        {
            logger.LogTrace(
                "GetBimsChildObjectsByAttributeAsync started with bimId: {@BimID}, folderId: {@FolderKey}, attributeId: {@AttributeID}",
                bimId,
                folderKey,
                attributeId);
            var data = new
            {
                folderKey = folderKey,
                id = bimId,
                attributeId = attributeId,
                includeLastAction = false,
            };

            var response = await requestUtility.GetResponseAsync(Resources.MethodBimChildrenByAttribute, data);
            return response.ToObject<List<ObjectBase>>();
        }

        public async Task<bool> DownloadLastVersionAsync(string bimId, string path)
        {
            logger.LogTrace("DownloadLastVersionAsync started with bimId: {@BimID}, path: {@Path}", bimId, path);
            var lastVersion = await GetBimLastVersion(bimId);
            logger.LogDebug("Received version: {@ObjectVersion}", lastVersion);
            if (lastVersion == null)
                return false;

            var lastVersionBimObject = await commonRequests.GetObjectAsync(lastVersion.ID.Value);
            logger.LogDebug("Received version of bim object: {@ObjectVersion}", lastVersion);

            // Download file
            var fileDetails = lastVersionBimObject.Values.Files.FirstOrDefault();
            if (fileDetails?.ID == null || fileDetails?.FileName == null)
                return false;

            var pathToDownload = $"{path}{fileDetails.FileName}";
            logger.LogDebug("Path to download: {@Path}", pathToDownload);
            var downloadResult = await commonRequests.DownloadFileAsync(fileDetails.ID.Value, pathToDownload);
            logger.LogDebug("Downloaded: {@IsSuccess}", downloadResult);

            return downloadResult;
        }

        public async Task<ObjectBaseCreateResult> UpdateBimVersion(string bimId, string filePath)
        {
            logger.LogTrace("UpdateBimVersion started with bimId: {@BimID}, path: {@Path}", bimId, filePath);
            var lastVersion = await GetBimLastVersion(bimId);
            logger.LogDebug("Received version: {@ObjectVersion}", lastVersion);
            var versionTypeId = lastVersion?.Values?.Type?.ID;
            var lastVersionNumber = lastVersion?.Values?.BimVersionNum;
            if (!int.TryParse(bimId, out var parsedId) || versionTypeId == null || lastVersionNumber == null)
                return null;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var uploadedFile = await commonRequests.AddFileAsync(fileName, filePath);
            logger.LogDebug("Uploaded file: {@FileInfo}", uploadedFile);
            if (uploadedFile?.IsSuccess == null || !uploadedFile.IsSuccess.Value)
                return null;

            var addedId = uploadedFile.ID.Value;
            var newVersionNumber = lastVersionNumber + 1;
            var fileToUpload = new ObjectBaseToCreate
            {
                FileIds = new int[] { addedId },
                Values = new ObjectBaseValueToCreate
                {
                    Name = $"{fileName} ver.{newVersionNumber}",
                    Type = versionTypeId,
                    Favorites = string.Empty,
                    BimVersionNum = newVersionNumber,
                    ParentModel = parsedId,
                },
            };

            var updatedBim = await commonRequests.CreateObjectAsync(fileToUpload);
            logger.LogDebug("Created bim: {@BimInfo}", updatedBim);
            return updatedBim;
        }

        public async Task<ObjectBase> GetBimLastVersion(string bimId)
        {
            logger.LogTrace("DownloadLastVersionAsync started with bimId: {@BimID}", bimId);

            // Find file folder
            var folderKey = await commonRequests.GetDefaultObjectTypeFolderKeyAsync(OBJECTTYPE_BIM);
            logger.LogDebug("Received folder key: {@FolderKey}", folderKey);

            // Find children corresponding to different version
            var bimCategoryId = await commonRequests.GetCategoryId(OBJECTTYPE_BIM);
            logger.LogDebug("Received category id: {@CategoryID}", bimCategoryId);
            var bimTypePossibleAttributes = await commonRequests.GetTypesAttributesDefinitionAsync(bimCategoryId.ToString());
            logger.LogDebug("Received attributes: {@Attributes}", bimTypePossibleAttributes);
            var versionAttributeDefinitionId = bimTypePossibleAttributes.FirstOrDefault(a => a.Field == OBJECTTYPE_BIM_ATTRIBUTE_VERSION);
            if (versionAttributeDefinitionId?.AttrId == null)
                return null;

            var bimChildren = await GetBimsChildObjectsByAttributeAsync(bimId, folderKey, versionAttributeDefinitionId.AttrId.ToString());
            logger.LogDebug("Received attributes: {@Attributes}", bimTypePossibleAttributes);

            // Get last version from children as ObjectBase
            var lastVersionNumber = bimChildren.Max(c => c.Values.BimVersionNum);
            var lastVersion = bimChildren.First(c => c.Values.BimVersionNum == lastVersionNumber);

            return lastVersion;
        }

        public async Task<IEnumerable<int>> DeleteBimAsync(int bimId)
        {
            logger.LogTrace("DeleteBimAsync started with bimId: {@BimID}", bimId);
            return await commonRequests.DeleteObjectAsync(bimId);
        }
    }
}
