using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Connection.LementPro.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connection.LementPro.Tests
{
    [SuppressMessage("ReSharper", "ContextualLoggerProblem", Justification = "Only for test")]
    internal class CommonRequestsUtilityTester : CommonRequestsUtility
    {
        public CommonRequestsUtilityTester(
            HttpRequestUtility requestUtility,
            ILogger<CommonRequestsUtility> logger)
            : base(requestUtility, logger)
        {
        }

        public new Task<List<Category>> GetMenuCategoriesAsync()
            => base.GetMenuCategoriesAsync();

        public new Task<List<Folder>> GetFoldersTreeAsync(string categoryObject, int categoryId)
            => base.GetFoldersTreeAsync(categoryObject, categoryId);

        public new Task<string> GetDefaultObjectTypeFolderKeyAsync(string objectType)
            => base.GetDefaultObjectTypeFolderKeyAsync(objectType);

        public new Task<int> GetCategoryId(string objectType)
            => base.GetCategoryId(objectType);

        public new Task<List<LementProType>> GetObjectsTypes(string objectType)
            => base.GetObjectsTypes(objectType);

        public new Task<List<ObjectBase>> GetObjectsListFromFolderAsync(string objectType, string folderKey)
            => base.GetObjectsListFromFolderAsync(objectType, folderKey);

        public new Task<IEnumerable<ObjectBase>> RetrieveObjectsListAsync(string objectType)
            => base.RetrieveObjectsListAsync(objectType);

        public new Task<ObjectBase> GetObjectAsync(int objectId)
            => base.GetObjectAsync(objectId);

        public new Task<ObjectBaseCreateResult> CreateObjectAsync(ObjectBaseToCreate objectToCreate)
            => base.CreateObjectAsync(objectToCreate);

        public new Task<List<LementProType>> GetTypesAsync()
            => base.GetTypesAsync();

        public new Task<ObjectBase> ArchiveObjectAsync(int objectId)
            => base.ArchiveObjectAsync(objectId);

        public new Task<List<TypeAttribute>> GetTypesAttributesDefinitionAsync(string typeId)
            => base.GetTypesAttributesDefinitionAsync(typeId);

        public new int GetFolderKeysId(string source)
            => base.GetFolderKeysId(source);

        public new Task<bool> DownloadFileAsync(int fileId, string filePath)
            => base.DownloadFileAsync(fileId, filePath);

        public new Task<ObjectBaseCreateResult> UploadFileAsync(int objectType, string fileName, string filePath)
            => base.UploadFileAsync(objectType, fileName, filePath);

        public new Task<JToken> GetDefaultTemplate(int categoryId, int typeId)
            => base.GetDefaultTemplate(categoryId, typeId);

        public new Task<List<int>> DeleteObjectAsync(int objectId)
            => base.DeleteObjectAsync(objectId);

        public new bool IsLargeFile(string filePath)
            => base.IsLargeFile(filePath);

        public new Task<ObjectBaseCreateResult> AddSmallFileAsync(string fileName, string filePath)
            => base.AddSmallFileAsync(fileName, filePath);

        public new Task<ObjectBaseCreateResult> AddLargeFileAsync(string fileName, string filePath)
            => base.AddLargeFileAsync(fileName, filePath);

        public new Task<ObjectBaseCreateResult> BeginUploadAsync(string fileName, byte[] filePart, long totalSize)
            => base.BeginUploadAsync(fileName, filePart, totalSize);

        public new Task<ObjectBaseCreateResult> UploadPartAsync(
            string fileName,
            int id,
            bool endUpload,
            byte[] filePart)
            => base.UploadPartAsync(fileName, id, endUpload, filePart);

        public new Task<ObjectBaseCreateResult> EndUploadAsync(int id)
            => base.EndUploadAsync(id);

        public new Task<ObjectBaseCreateResult> SendUploadAsync(
            string url,
            Dictionary<string, string> data,
            byte[] filePart,
            string fileName)
            => base.SendUploadAsync(url, data, filePart, fileName);

    }
}
