using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;
using File = System.IO.File;

namespace MRS.DocumentManagement.Connection.LementPro.Utilities
{
    /// <summary>
    /// Utility with request common for services for common models as such as ObjectBase, Catetogry etc.
    /// </summary>
    public class CommonRequestsUtility : IDisposable
    {
        // ctor used for tests.
        public CommonRequestsUtility()
        {
        }

        public CommonRequestsUtility(HttpRequestUtility requestUtility)
            => RequestUtility = requestUtility;

        // Marked as virtual to be mockable
        protected virtual HttpRequestUtility RequestUtility { get; set; }

        public void Dispose()
            => RequestUtility?.Dispose();

        public async Task<ObjectBaseCreateResult> AddFileAsync(string fileName, string filePath)
        {
            if (IsLargeFile(filePath))
                return await AddLargeFileAsync(fileName, filePath);

            return await AddSmallFileAsync(fileName, filePath);
        }

        protected internal async Task<List<Category>> GetMenuCategoriesAsync()
        {
            var response = await RequestUtility.GetResponseWithoutDataAsync(Resources.MethodCategoriesGetMenuList);
            return response.ToObject<List<Category>>();
        }

        protected internal async Task<List<Folder>> GetFoldersTreeAsync(string categoryObject, int categoryId)
        {
            // Works only with native categories as Project or Task. Custom types should use different url.
            var url = string.Format(Resources.MethodFolderGetListForCategory, categoryObject);

            var data = new
            {
                categoryId = categoryId,
                includeCounts = false,
            };

            var response = await RequestUtility.GetResponseAsync(url, data);
            return response.ToObject<List<Folder>>();
        }

        protected internal async Task<string> GetDefaultObjectTypeFolderKeyAsync(string objectType)
        {
            var objectCategoryId = await GetCategoryId(objectType);
            var objectsFoldersTree = await GetFoldersTreeAsync(objectType, objectCategoryId);

            // Choose first folder as default
            return objectsFoldersTree.FirstOrDefault()?.FolderKey;
        }

        protected internal async Task<int> GetCategoryId(string objectType)
        {
            var categories = await GetMenuCategoriesAsync();

            // Starts with has choosen because some types in URL can be plural
            var objectCategoryId = categories.FirstOrDefault(c => c.Url.StartsWith($"/{objectType}")).ID;

            return objectCategoryId.Value;
        }

        protected internal async Task<List<LementProType>> GetObjectsTypes(string objectType)
        {
            var objectsCategory = await GetCategoryId(objectType);
            var types = await GetTypesAsync();
            var objectTypes = types.FirstOrDefault(t => t.ID == objectsCategory);
            return objectTypes.Items;
        }

        protected internal async Task<List<ObjectBase>> GetObjectsListFromFolderAsync(string objectType, string folderKey)
        {
            string url = string.Format(Resources.MethodGetObjectsList, objectType);
            var data = new { folderKey = folderKey };
            var response = await RequestUtility.GetResponseAsync(url, data);

            // Response contains list of object and their count. Pick list
            return response[RESPONSE_COLLECTION_ITEMS_NAME].ToObject<List<ObjectBase>>();
        }

        protected internal async Task<IEnumerable<ObjectBase>> RetriveObjectsListAsync(string objectType)
        {
            var folderKey = await GetDefaultObjectTypeFolderKeyAsync(objectType);
            var objectsList = await GetObjectsListFromFolderAsync(objectType, folderKey);
            return objectsList;
        }

        protected internal async Task<ObjectBase> GetObjectAsync(int objectId)
        {
            var data = new
            {
                id = objectId,
                rememberme = true,
            };

            var response = await RequestUtility.GetResponseAsync(Resources.MethodObjectGet, data);

            // Response contains some metadata and object
            return response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
        }

        protected internal async Task<ObjectBaseCreateResult> CreateObjectAsync(ObjectBaseToCreate objectToCreate)
        {
            var response = await RequestUtility.GetResponseAsync(Resources.MethodObjectCreate, objectToCreate);
            var createResult = response.ToObject<ObjectBaseCreateResult>();
            return createResult;
        }

        protected internal async Task<List<LementProType>> GetTypesAsync()
        {
            var response = await RequestUtility.GetResponseWithoutDataAsync(Resources.MethodTypesGetTree);
            var typesTree = response.ToObject<List<LementProType>>();

            // Types tree has one root with all types included in Items
            return typesTree.FirstOrDefault().Items;
        }

        protected internal async Task<bool> ArchiveObjectAsync(int objectId)
        {
            var data = new { id = objectId };

            try
            {
                await RequestUtility.GetResponseAsync(Resources.MethodObjectArchive, data);
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected internal async Task<List<TypeAttribute>> GetTypesAttributesDefinitionAsync(string typeId)
        {
            var data = new
            {
                typeId = typeId,
                includeInherited = false,
            };

            var response = await RequestUtility.GetResponseAsync(Resources.MethodTypesGetAttributesDefinition, data);
            return response.ToObject<List<TypeAttribute>>();
        }

        protected internal int GetFolderKeysId(string source)
        {
            var folderKey = JToken.Parse(source).ToObject<FolderKey>();
            return folderKey.ID.Value;
        }

        // TODO: MB move this method (and future upload method) to separate FileService?
        protected internal async Task<bool> DownloadFileAsync(string fileId, string filePath)
        {
            var data = new { fileId = fileId };
            var url = Resources.MethodFileDownload;
            await using var output = File.OpenWrite(filePath);
            try
            {
                await using var fileStream = await RequestUtility.GetResponseStreamAsync(url, data);
                await fileStream.CopyToAsync(output);
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected internal async Task<ObjectBaseCreateResult> UploadFileAsync(int objectType, string fileName, string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var addResult = await AddFileAsync(fileName, filePath);

            if (addResult == null || !addResult.IsSuccess.Value || !addResult.ID.HasValue)
                return null;

            var addedId = addResult.ID.Value;
            var fileToUpload = new ObjectBaseToCreate
            {
                FileIds = new int[] { addedId },
                Values = new ObjectBaseValueToCreate
                {
                    Name = fileName,
                    Type = objectType,
                    Favorites = string.Empty,
                },
            };

            return await CreateObjectAsync(fileToUpload);
        }

        protected internal async Task<JToken> GetDefaultTemplate(int categoryId, int typeId)
        {
            var url = Resources.MethodObjectGetDefaultTemplate;
            var data = new
            {
                categoryId,
                typeId,
            };

            var response = await RequestUtility.GetResponseAsync(url, data);

            return response;
        }

        protected internal async Task<List<int>> DeleteObjectAsync(int objectId)
        {
            var data = new { id = objectId };
            var response = await RequestUtility.GetResponseAsync(Resources.MethodObjectDelete, data);

            return response.ToObject<List<int>>();
        }

        protected bool IsLargeFile(string filePath)
        {
            var fileSize = new FileInfo(filePath).Length;
            return fileSize >= UPLOAD_FILES_CHUNKS_SIZE;
        }

        protected async Task<ObjectBaseCreateResult> AddSmallFileAsync(string fileName, string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var totalSize = stream.Length;
            var firstChunk = new byte[totalSize];
            stream.Read(firstChunk);
            var beginUploadResult = await BeginUploadAsync(fileName, firstChunk, totalSize);
            var uploadId = beginUploadResult.ID;

            var endUploadResult = await EndUploadAsync(uploadId.Value);
            if (!endUploadResult.IsSuccess.GetValueOrDefault())
                return new ObjectBaseCreateResult { IsSuccess = false };

            return beginUploadResult;
        }

        protected async Task<ObjectBaseCreateResult> AddLargeFileAsync(string fileName, string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var firstChunk = new byte[UPLOAD_FILES_CHUNKS_SIZE];
            var totalSize = stream.Length;
            stream.Read(firstChunk);
            var beginUploadResult = await BeginUploadAsync(fileName, firstChunk, totalSize);
            if (beginUploadResult?.IsSuccess == null || !beginUploadResult.IsSuccess.Value)
                return null;

            var uploadId = beginUploadResult.ID;

            while (stream.CanRead && stream.Position != stream.Length)
            {
                var notReadedByteLength = stream.Length - stream.Position;

                // For some reason Lement Pro returns error if last chunk of some of the IFCs has not triple size
                var isLastChunk = notReadedByteLength <= (UPLOAD_FILES_CHUNKS_SIZE * 3);
                var chunkSize =
                    isLastChunk
                    ? notReadedByteLength
                    : UPLOAD_FILES_CHUNKS_SIZE;

                var chunk = new byte[chunkSize];
                stream.Read(chunk);

                var uploadPartResult = await UploadPartAsync(fileName, uploadId.Value, isLastChunk, chunk);
                if (!uploadPartResult.IsSuccess.GetValueOrDefault())
                    return new ObjectBaseCreateResult { IsSuccess = false };
            }

            return beginUploadResult;
        }

        protected async Task<ObjectBaseCreateResult> BeginUploadAsync(string fileName, byte[] filePart, long totalSize)
        {
            var url = Resources.MethodObjectFileBeginUpload;
            var data = new Dictionary<string, string>
            {
                { REQUEST_UPLOAD_FILENAME_FIELDNAME, fileName },
                { REQUEST_UPLOAD_SIZE_FIELDNAME, totalSize.ToString() },
            };

            var response = await SendUploadAsync(url, data, filePart, fileName);
            return response;
        }

        protected async Task<ObjectBaseCreateResult> UploadPartAsync(string fileName, int id, bool endUpload, byte[] filePart)
        {
            var url = Resources.MethodObjectFileUploadPart;
            var data = new Dictionary<string, string>
            {
                { REQUEST_UPLOAD_ID_FIELDNAME, id.ToString() },
                { REQUEST_UPLOAD_ENDUPLOAD_FIELDNAME, endUpload.ToString() },
            };

            var response = await SendUploadAsync(url, data, filePart, fileName);
            return response;
        }

        protected async Task<ObjectBaseCreateResult> EndUploadAsync(int id)
        {
            var url = Resources.MethodObjectFileEndUpload;
            var data = new { id = id };

            var response = await RequestUtility.GetResponseAsync(url, data);
            return response.ToObject<ObjectBaseCreateResult>();
        }

        protected async Task<ObjectBaseCreateResult> SendUploadAsync(
            string url,
            Dictionary<string, string> data,
            byte[] filePart,
            string fileName)
        {
            var stream = new MemoryStream(filePart);
            var response = await RequestUtility
                .SendStreamWithDataAsync(url, stream, fileName, REQUEST_UPLOAD_FILEPART_FIELDNAME, data);

            return response.ToObject<ObjectBaseCreateResult>();
        }
    }
}
