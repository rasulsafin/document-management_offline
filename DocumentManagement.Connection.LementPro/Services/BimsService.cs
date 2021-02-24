using System;
using System.Collections.Generic;
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
            // Find file folder
            var folderKey = await commonRequests.GetDefaultObjectTypeFolderKeyAsync(OBJECTTYPE_BIM);

            // Find children corresponding to different version
            var bimCategoryId = await commonRequests.GetCategoryId(OBJECTTYPE_BIM);
            var bimTypePossibleAttributes = await commonRequests.GetTypesAttributesDefinitionAsync(bimCategoryId.ToString());
            var versionAttributeDefinitionId = bimTypePossibleAttributes.FirstOrDefault(a => a.Field == OBJECTTYPE_BIM_ATTRIBUTE_VERSION);
            if (versionAttributeDefinitionId?.AttrId == null)
                return false;

            var bimChildren = await GetBimsChildObjectsByAttributeAsync(bimId, folderKey, versionAttributeDefinitionId.AttrId.ToString());

            // Get last version from children as ObjectBase
            var lastVersionNumber = bimChildren.Max(c => c.Values.BimVersionNum);
            var lastVersion = bimChildren.First(c => c.Values.BimVersionNum == lastVersionNumber);
            var lastVersionObject = await commonRequests.GetObjectAsync(lastVersion.ID.Value);

            // Download file
            var fileDetails = lastVersionObject.Values.Files.FirstOrDefault();
            if (fileDetails?.ID == null || fileDetails?.FileName == null)
                return false;

            var pathToDownload = $"{path}{fileDetails.FileName}";
            var downloadResult = await commonRequests.DownloadFileAsync(fileDetails.ID.ToString(), pathToDownload);

            return downloadResult;
        }
    }
}
