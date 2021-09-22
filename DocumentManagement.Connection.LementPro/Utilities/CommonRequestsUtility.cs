using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Connection.LementPro.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Brio.Docs.Connection.LementPro.LementProConstants;
using File = System.IO.File;

namespace Brio.Docs.Connection.LementPro.Utilities
{
    /// <summary>
    /// Utility with request common for services for common models as such as ObjectBase, Catetogry etc.
    /// </summary>
    public class CommonRequestsUtility
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly ILogger<CommonRequestsUtility> logger;

        public CommonRequestsUtility(HttpRequestUtility requestUtility, ILogger<CommonRequestsUtility> logger)
        {
            this.requestUtility = requestUtility;
            this.logger = logger;
            logger.LogTrace("CommonRequestsUtility created");
        }

        public async Task<ObjectBaseCreateResult> AddFileAsync(string fileName, string filePath)
        {
            logger.LogTrace("AddFileAsync started with fileName: {Name}, filePath: {Path}", fileName, filePath);
            if (IsLargeFile(filePath))
                return await AddLargeFileAsync(fileName, filePath);

            return await AddSmallFileAsync(fileName, filePath);
        }

        protected internal async Task<List<Category>> GetMenuCategoriesAsync()
        {
            logger.LogTrace("GetMenuCategoriesAsync started");
            var response = await requestUtility.GetResponseWithoutDataAsync(Resources.MethodCategoriesGetMenuList);
            logger.LogDebug("Received response: {@Data}", response);
            return response.ToObject<List<Category>>();
        }

        protected internal async Task<List<Folder>> GetFoldersTreeAsync(string categoryObject, int categoryId)
        {
            logger.LogTrace(
                "GetFoldersTreeAsync started with categoryObject: {CategoryObject}, categoryId: {CategoryID}",
                categoryId,
                categoryId);

            // Works only with native categories as Project or Task. Custom types should use different url.
            var url = string.Format(Resources.MethodFolderGetListForCategory, categoryObject);

            var data = new
            {
                categoryId = categoryId,
                includeCounts = false,
            };

            var response = await requestUtility.GetResponseAsync(url, data);
            logger.LogDebug("Received response: {@Data}", response);
            return response.ToObject<List<Folder>>();
        }

        protected internal async Task<string> GetDefaultObjectTypeFolderKeyAsync(string objectType)
        {
            logger.LogTrace("GetDefaultObjectTypeFolderKeyAsync started with objectType: {@ObjectType}", objectType);
            var objectCategoryId = await GetCategoryId(objectType);
            logger.LogDebug("{@ObjectType}: [{CategoryID}]", objectType, objectCategoryId);
            var objectsFoldersTree = await GetFoldersTreeAsync(objectType, objectCategoryId);
            logger.LogDebug("Received folder tree: {@Tree}", objectsFoldersTree);

            // Choose first folder as default
            return objectsFoldersTree.FirstOrDefault()?.FolderKey;
        }

        protected internal async Task<int> GetCategoryId(string objectType)
        {
            logger.LogTrace("GetCategoryId started with objectType: {@ObjectType}", objectType);
            var categories = await GetMenuCategoriesAsync();
            logger.LogTrace("Received categories: {@Categories}", categories);

            // Starts with has chosen because some types in URL can be plural
            var objectCategoryId = categories.FirstOrDefault(c => c.Url.StartsWith($"/{objectType}")).ID;

            return objectCategoryId.Value;
        }

        protected internal async Task<List<LementProType>> GetObjectsTypes(string objectType)
        {
            logger.LogTrace("GetObjectsTypes started with objectType: {ObjectType}", objectType);
            var objectsCategory = await GetCategoryId(objectType);
            logger.LogDebug("{@ObjectType}: [{CategoryID}]", objectType, objectsCategory);
            var types = await GetTypesAsync();
            logger.LogDebug("Received types: {@Types}", types);
            var objectTypes = types.FirstOrDefault(t => t.ID == objectsCategory);
            logger.LogDebug("Found types: {@Types}", objectTypes);
            return objectTypes.Items;
        }

        protected internal async Task<List<ObjectBase>> GetObjectsListFromFolderAsync(string objectType, string folderKey)
        {
            logger.LogTrace(
                "GetObjectsListFromFolderAsync started with objectType: {ObjectType}, folderKey: {FolderKey}",
                objectType,
                folderKey);
            string url = string.Format(Resources.MethodGetObjectsList, objectType);
            var data = new { folderKey = folderKey };
            var response = await requestUtility.GetResponseAsync(url, data);
            logger.LogDebug("Received response: {@Data}", response);

            // Response contains list of object and their count. Pick list
            return response[RESPONSE_COLLECTION_ITEMS_NAME].ToObject<List<ObjectBase>>();
        }

        protected internal async Task<IEnumerable<ObjectBase>> RetrieveObjectsListAsync(string objectType)
        {
            logger.LogTrace("GetObjectsListFromFolderAsync started with objectType: {ObjectType}", objectType);
            var folderKey = await GetDefaultObjectTypeFolderKeyAsync(objectType);
            logger.LogDebug("Received folder key: {@FolderKey}", folderKey);
            var objectsList = await GetObjectsListFromFolderAsync(objectType, folderKey);
            logger.LogDebug("Received objects list: {@Objects}", objectsList);
            return objectsList;
        }

        protected internal async Task<ObjectBase> GetObjectAsync(int objectId)
        {
            logger.LogTrace("GetObjectAsync started with objectId: {@ObjectID}", objectId);
            var data = new
            {
                id = objectId,
                rememberme = true,
            };

            var response = await requestUtility.GetResponseAsync(Resources.MethodObjectGet, data);
            logger.LogDebug("Received response: {@Data}", response);

            // Response contains some metadata and object
            return response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
        }

        protected internal async Task<ObjectBaseCreateResult> CreateObjectAsync(ObjectBaseToCreate objectToCreate)
        {
            logger.LogTrace("CreateObjectAsync started with objectToCreate: {@Object}", objectToCreate);
            var response = await requestUtility.GetResponseAsync(Resources.MethodObjectCreate, objectToCreate);
            logger.LogDebug("Received response: {@Data}", response);
            var createResult = response.ToObject<ObjectBaseCreateResult>();
            return createResult;
        }

        protected internal async Task<List<LementProType>> GetTypesAsync()
        {
            logger.LogTrace("GetTypesAsync started");
            var response = await requestUtility.GetResponseWithoutDataAsync(Resources.MethodTypesGetTree);
            logger.LogDebug("Received response: {@Data}", response);
            var typesTree = response.ToObject<List<LementProType>>();

            // Types tree has one root with all types included in Items
            return typesTree.FirstOrDefault().Items;
        }

        protected internal async Task<ObjectBase> ArchiveObjectAsync(int objectId)
        {
            logger.LogTrace("ArchiveObjectAsync started with objectId: {ObjectID}", objectId);
            var data = new { id = objectId };

            try
            {
                await requestUtility.GetResponseAsync(Resources.MethodObjectArchive, data);
                var deleted = await GetObjectAsync(objectId);
                logger.LogDebug("Deleted object: {@Object}", deleted);

                return deleted;
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Can't archive object {ObjectID}", objectId);
                return null;
            }
        }

        protected internal async Task<List<TypeAttribute>> GetTypesAttributesDefinitionAsync(string typeId)
        {
            logger.LogTrace("GetTypesAttributesDefinitionAsync started with typeId: {TypeId}", typeId);

            var data = new
            {
                typeId = typeId,
                includeInherited = false,
            };

            var response = await requestUtility.GetResponseAsync(Resources.MethodTypesGetAttributesDefinition, data);
            logger.LogDebug("Received response: {@Data}", response);
            return response.ToObject<List<TypeAttribute>>();
        }

        protected internal int GetFolderKeysId(string source)
        {
            logger.LogTrace("GetFolderKeysId started with source: {Source}", source);
            var folderKey = JToken.Parse(source).ToObject<FolderKey>();
            logger.LogDebug("Parsed folder key: {@FolderKey}", folderKey);
            return folderKey.ID.Value;
        }

        // TODO: MB move this method (and future upload method) to separate FileService?
        protected internal async Task<bool> DownloadFileAsync(int fileId, string filePath)
        {
            logger.LogTrace("DownloadFileAsync started with fileId: {FileID}, filePath: {Path}", fileId, filePath);
            var data = new { fileId = fileId };
            var url = Resources.MethodFileDownload;
            await using var output = File.OpenWrite(filePath);
            try
            {
                await using var fileStream = await requestUtility.GetResponseStreamAsync(url, data);
                await fileStream.CopyToAsync(output);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Can't download file {FileID} to {Path}", fileId, filePath);
                return false;
            }

            return true;
        }

        protected internal async Task<ObjectBaseCreateResult> UploadFileAsync(int objectType, string fileName, string filePath)
        {
            logger.LogTrace(
                "UploadFileAsync started with objectType: {ObjectType}, fileName: {FileName}, filePath: {Path}",
                objectType,
                fileName,
                filePath);
            if (!File.Exists(filePath))
                return null;

            var addResult = await AddFileAsync(fileName, filePath);
            logger.LogDebug("Added file: {@Item}", addResult);

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
            logger.LogTrace(
                "GetDefaultTemplate started with categoryId: {CategoryId}, typeId: {TypeId}",
                categoryId,
                typeId);
            var url = Resources.MethodObjectGetDefaultTemplate;
            var data = new
            {
                categoryId,
                typeId,
            };

            var response = await requestUtility.GetResponseAsync(url, data);
            logger.LogDebug("Received response: {@Data}", response);

            return response;
        }

        protected internal async Task<List<int>> DeleteObjectAsync(int objectId)
        {
            logger.LogTrace("DeleteObjectAsync started with objectId: {ObjectID}", objectId);
            var data = new { id = objectId };
            var response = await requestUtility.GetResponseAsync(Resources.MethodObjectDelete, data);
            logger.LogDebug("Received response: {@Data}", response);

            return response.ToObject<List<int>>();
        }

        protected bool IsLargeFile(string filePath)
        {
            logger.LogTrace("IsLargeFile started with filePath: {Path}", filePath);
            var fileSize = new FileInfo(filePath).Length;
            return fileSize >= UPLOAD_FILES_CHUNKS_SIZE;
        }

        protected async Task<ObjectBaseCreateResult> AddSmallFileAsync(string fileName, string filePath)
        {
            logger.LogTrace("AddSmallFileAsync started with fileName: {Name}, filePath: {Path}", fileName, filePath);
            using var stream = File.OpenRead(filePath);
            var totalSize = stream.Length;
            var firstChunk = new byte[totalSize];
            stream.Read(firstChunk);
            var beginUploadResult = await BeginUploadAsync(fileName, firstChunk, totalSize);
            logger.LogDebug("Response for begging upload: {@Data}", beginUploadResult);
            var uploadId = beginUploadResult.ID;

            var endUploadResult = await EndUploadAsync(uploadId.Value);
            logger.LogDebug("Response for ending upload: {@Data}", endUploadResult);
            if (!endUploadResult.IsSuccess.GetValueOrDefault())
                return new ObjectBaseCreateResult { IsSuccess = false };

            return beginUploadResult;
        }

        protected async Task<ObjectBaseCreateResult> AddLargeFileAsync(string fileName, string filePath)
        {
            logger.LogTrace("AddLargeFileAsync started with fileName: {Name}, filePath: {Path}", fileName, filePath);
            using var stream = File.OpenRead(filePath);
            var firstChunk = new byte[UPLOAD_FILES_CHUNKS_SIZE];
            var totalSize = stream.Length;
            stream.Read(firstChunk);
            var beginUploadResult = await BeginUploadAsync(fileName, firstChunk, totalSize);
            logger.LogDebug("Response for begging upload: {@Data}", beginUploadResult);
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
                logger.LogDebug("Response for uploading part: {@Data}", uploadPartResult);
                if (!uploadPartResult.IsSuccess.GetValueOrDefault())
                    return new ObjectBaseCreateResult { IsSuccess = false };
            }

            return beginUploadResult;
        }

        protected async Task<ObjectBaseCreateResult> BeginUploadAsync(string fileName, byte[] filePart, long totalSize)
        {
            logger.LogTrace("BeginUploadAsync started with fileName: {Name}, totalSize: {Size}", fileName, totalSize);
            var url = Resources.MethodObjectFileBeginUpload;
            var data = new Dictionary<string, string>
            {
                { REQUEST_UPLOAD_FILENAME_FIELDNAME, fileName },
                { REQUEST_UPLOAD_SIZE_FIELDNAME, totalSize.ToString() },
            };

            var response = await SendUploadAsync(url, data, filePart, fileName);
            logger.LogDebug("Received response: {@Data}", response);
            return response;
        }

        protected async Task<ObjectBaseCreateResult> UploadPartAsync(string fileName, int id, bool endUpload, byte[] filePart)
        {
            logger.LogTrace(
                "UploadPartAsync started with fileName: {Name}, id: {LoadingID}, endUpload: {IsEnd}",
                fileName,
                id,
                endUpload);
            var url = Resources.MethodObjectFileUploadPart;
            var data = new Dictionary<string, string>
            {
                { REQUEST_UPLOAD_ID_FIELDNAME, id.ToString() },
                { REQUEST_UPLOAD_ENDUPLOAD_FIELDNAME, endUpload.ToString() },
            };

            var response = await SendUploadAsync(url, data, filePart, fileName);
            logger.LogDebug("Received response: {@Data}", response);
            return response;
        }

        protected async Task<ObjectBaseCreateResult> EndUploadAsync(int id)
        {
            logger.LogTrace("EndUploadAsync started with id: {LoadingID}", id);
            var url = Resources.MethodObjectFileEndUpload;
            var data = new { id = id };

            var response = await requestUtility.GetResponseAsync(url, data);
            logger.LogDebug("Received response: {@Data}", response);
            return response.ToObject<ObjectBaseCreateResult>();
        }

        protected async Task<ObjectBaseCreateResult> SendUploadAsync(
            string url,
            Dictionary<string, string> data,
            byte[] filePart,
            string fileName)
        {
            logger.LogTrace("SendUploadAsync started with data: {@Data}, fileName: {Name}", data, fileName);
            var stream = new MemoryStream(filePart);
            var response = await requestUtility
                .SendStreamWithDataAsync(url, stream, fileName, REQUEST_UPLOAD_FILEPART_FIELDNAME, data);
            logger.LogDebug("Received response: {@Data}", response);
            return response.ToObject<ObjectBaseCreateResult>();
        }
    }
}
