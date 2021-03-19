using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class BimsService : IDisposable
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly CommonRequestsUtility commonRequests;

        public BimsService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests)
        {
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
        }

        public void Dispose()
        {
            requestUtility.Dispose();
            commonRequests.Dispose();
        }

        public async Task<IEnumerable<ObjectBase>> GetAllBimFilesAsync()
            => await commonRequests.RetriveObjectsListAsync(OBJECTTYPE_BIM);

        public async Task<List<BimAttribute>> GetBimAttributesAsync(string bimId, string folderId)
        {
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
            var lastVersion = await GetBimLastVersion(bimId);
            if (lastVersion == null)
                return false;

            var lastVersionBimObject = await commonRequests.GetObjectAsync(lastVersion.ID.Value);

            // Download file
            var fileDetails = lastVersionBimObject.Values.Files.FirstOrDefault();
            if (fileDetails?.ID == null || fileDetails?.FileName == null)
                return false;

            var pathToDownload = $"{path}{fileDetails.FileName}";
            var downloadResult = await commonRequests.DownloadFileAsync(fileDetails.ID.Value, pathToDownload);

            return downloadResult;
        }

        public async Task<ObjectBaseCreateResult> UpdateBimVersion(string bimId, string filePath)
        {
            var lastVersion = await GetBimLastVersion(bimId);
            var versionTypeId = lastVersion?.Values?.Type?.ID;
            var lastVersionNumber = lastVersion?.Values?.BimVersionNum;
            if (!int.TryParse(bimId, out var parsedId) || versionTypeId == null || lastVersionNumber == null)
                return null;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var uploadedFile = await commonRequests.AddFileAsync(fileName, filePath);
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
            return updatedBim;
        }

        public async Task<ObjectBase> GetBimLastVersion(string bimId)
        {
            // Find file folder
            var folderKey = await commonRequests.GetDefaultObjectTypeFolderKeyAsync(OBJECTTYPE_BIM);

            // Find children corresponding to different version
            var bimCategoryId = await commonRequests.GetCategoryId(OBJECTTYPE_BIM);
            var bimTypePossibleAttributes = await commonRequests.GetTypesAttributesDefinitionAsync(bimCategoryId.ToString());
            var versionAttributeDefinitionId = bimTypePossibleAttributes.FirstOrDefault(a => a.Field == OBJECTTYPE_BIM_ATTRIBUTE_VERSION);
            if (versionAttributeDefinitionId?.AttrId == null)
                return null;

            var bimChildren = await GetBimsChildObjectsByAttributeAsync(bimId, folderKey, versionAttributeDefinitionId.AttrId.ToString());

            // Get last version from children as ObjectBase
            var lastVersionNumber = bimChildren.Max(c => c.Values.BimVersionNum);
            var lastVersion = bimChildren.First(c => c.Values.BimVersionNum == lastVersionNumber);

            return lastVersion;
        }

        public async Task<IEnumerable<int>> DeleteBimAsync(int bimId)
            => await commonRequests.DeleteObjectAsync(bimId);
    }
}
