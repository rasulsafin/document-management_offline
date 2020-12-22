using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DocumentManagement.Connection.YandexDisk
{
    /// <summary>Нижний уровень взаимодействия с сервисом YandexDisk</summary>
    public class YandexDiskController
    {

        public static CoolLogger logger = new CoolLogger("controller");


        private string accessToken;


        public YandexDiskController(string accessToken)
        {
            this.accessToken = accessToken;

        }

        #region PROPFIND 
        public async Task<IEnumerable<DiskElement>> GetListAsync(string path = "/")
        {
            try
            {
                HttpWebRequest request = YandexHelper.RequestGetList(accessToken, path);
                WebResponse response = await request.GetResponseAsync();
                XmlDocument xml = new XmlDocument();
                using (Stream stream = response.GetResponseStream())
                {
                    using (XmlReader xmlReader = XmlReader.Create(stream))
                        xml.Load(xmlReader);
                }
                response.Close();
                List<DiskElement> items = DiskElement.GetElements(xml.DocumentElement);
                return items;
            }
            catch (WebException web)
            {
                logger.Message($"Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }
        }
        #endregion

        #region Create Directory
        public async Task<bool> CreateDirAsync(string path, string nameDir)
        {
            try
            {
                string newPath = YandexHelper.DirectoryName(path, nameDir);
                HttpWebRequest request = YandexHelper.RequestCreateDir(accessToken, newPath);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse http)
                    {
                        if (http.StatusCode == HttpStatusCode.Created)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (WebException web)
            {
                logger.Message($"Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }
        }
        #endregion

        #region Download File
        /// <summary>
        /// Скачивание файла (GET)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="currentPath"></param>
        /// <param name="updateProgress"></param>
        /// <returns></returns>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/get.html/</remarks>
        public async Task<bool> DownloadFileAsync(string path, string currentPath, Action<ulong, ulong> updateProgress = null)
        {
            try
            {
                //string newPath = YandexHelper.NewPath(path, nameDir);
                logger.Message($"path={path}; currentPath={currentPath}; ");
                HttpWebRequest request = YandexHelper.RequestDownloadFile(accessToken, path);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    var length = response.ContentLength;
                    logger.Message($"length={length}");
                    using (var writer = File.OpenWrite(currentPath))
                    {
                        using (var reader = response.GetResponseStream())
                        {
                            const int BUFFER_LENGTH = 4096;
                            var total = (ulong)response.ContentLength;
                            ulong current = 0;
                            var buffer = new byte[BUFFER_LENGTH];
                            var count = reader.Read(buffer, 0, BUFFER_LENGTH);
                            while (count > 0)
                            {
                                writer.Write(buffer, 0, count);
                                current += (ulong)count;
                                updateProgress?.Invoke(current, total);
                                count = reader.Read(buffer, 0, BUFFER_LENGTH);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (DirectoryNotFoundException)
            {
                logger.Message($"Директория не создана, не могу записать файл");
                throw;
            }
            catch (WebException web)
            {
                logger.Message($"Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                //throw;
                return false;
            }
        } 
        #endregion

        #region Delete file and directory
        public async Task<bool> DeleteAsync(string path)
        {
            try
            {
                HttpWebRequest request = YandexHelper.RequestDelete(accessToken, path);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse http)
                    {
                        if (http.StatusCode == HttpStatusCode.OK)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (WebException web)
            {
                logger.Message($"Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }
        }
        #endregion

        #region Load File
        /// <summary>
        /// Загрузить файл на сервер
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="progressChenge"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        public async Task<bool> LoadFileAsync(string path, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            try
            {
                #region загрузка 
                //logger.Message($"path={path}; fileName={fileName}; ");
                FileInfo fileInfo = new FileInfo(fileName);

                string diskName = YandexHelper.FileName(path, fileInfo.Name);
                //logger.Message($"diskName={diskName}; ");

                HttpWebRequest request = YandexHelper.RequestLoadFile(accessToken, diskName);

                using (var reader = fileInfo.OpenRead())
                {
                    //logger.Message($"reader.Length={reader.Length};");
                    request.ContentLength = reader.Length;
                    using (var writer = request.GetRequestStream())
                    {
                        const int BUFFER_LENGTH = 4096;
                        var total = (ulong)reader.Length;
                        ulong current = 0;
                        var buffer = new byte[BUFFER_LENGTH];
                        var count = reader.Read(buffer, 0, BUFFER_LENGTH);
                        while (count > 0)
                        {
                            writer.Write(buffer, 0, count);
                            current += (ulong)count;
                            progressChenge?.Invoke(current, total);
                            count = reader.Read(buffer, 0, BUFFER_LENGTH);
                        }
                    }
                }

                #endregion

                #region прием                
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse httpResponse)
                    {
                        if (httpResponse.StatusCode == HttpStatusCode.Created) return true;
                        if (httpResponse.StatusCode == HttpStatusCode.InsufficientStorage) return false;
                        if (httpResponse.StatusCode == HttpStatusCode.Continue) return false;
                    }
                }
                #endregion
            }
            catch (WebException web)
            {
                logger.Message($"Exception Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }
            return false;
        }
        #endregion

        #region COPY TODO 
        /// <summary>
        /// Копирование (COPY)
        /// </summary>
        /// <param name="originPath"></param>
        /// <param name="copyPath"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html</remarks>
        public async Task<bool> CopyAsync(string originPath, string copyPath)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region MOVE TODO
        /// <summary>
        /// Перемещение и переименование (MOVE)
        /// </summary>
        /// <param name="originPath"></param>
        /// <param name="movePath"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html</remarks>
        public async Task<bool> MoveAsync(string originPath, string movePath)
        {
            try
            {                
                HttpWebRequest request = YandexHelper.RequestMove(accessToken, originPath, movePath);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse http)
                    {
                        if (http.StatusCode == HttpStatusCode.Created)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (WebException web)
            {
                logger.Message($"Exception Status={web.Status}");
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else
                {
                    logger.Error(web);
                    logger.Open();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }
        }
        #endregion
    }
}
