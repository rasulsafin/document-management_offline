﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    /// <summary>Нижний уровень взаимодействия с сервисом YandexDisk.</summary>
    public class YandexDiskController : IDiskController
    {
        // public static CoolLogger logger = new CoolLogger("controller");
        private string accessToken;

        public YandexDiskController(string accessToken)
        {
            this.accessToken = accessToken;
        }

        #region PROPFIND

        /// <summary>
        /// Возвращает список элементов.
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="FileNotFoundException"> Файл отсутвует </exception>
        /// <exception cref="TimeoutException" > Время ожидание вышло </exception>
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
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }

        public async Task<DiskElement> GetInfoAsync(string path = "/")
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
                DiskElement item = DiskElement.GetElement(xml.DocumentElement);
                return item;
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
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
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion
        #region Content

        public async Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null)
        {
            try
            {
                // загрузка
                HttpWebRequest request = YandexHelper.RequestLoadFile(accessToken, path);
                using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    // logger.Message($"reader.Length={reader.Length};");
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

                // прием
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse httpResponse)
                    {
                        if (httpResponse.StatusCode == HttpStatusCode.Created) return true;
                        if (httpResponse.StatusCode == HttpStatusCode.InsufficientStorage) return false;
                        if (httpResponse.StatusCode == HttpStatusCode.Continue) return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }

            return false;
        }

        /// <summary>
        /// Скачивает файл и возвращает его содержимое.
        /// </summary>
        /// <param name="path"> путь к файлу.</param>
        /// <param name="updateProgress"> прогресс </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="DirectoryNotFoundException">Директория не создана, не могу прочитать файл.</exception>
        /// <exception cref="FileNotFoundException">Файл не существует.</exception>
        public async Task<string> GetContentAsync(string path, Action<ulong, ulong> updateProgress = null)
        {
            try
            {
                HttpWebRequest request = YandexHelper.RequestDownloadFile(accessToken, path);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    var length = response.ContentLength;
                    StringBuilder builder = new StringBuilder();
                    using (var reader = response.GetResponseStream())
                    {
                        const int BUFFER_LENGTH = 4096;
                        var total = (ulong)response.ContentLength;
                        ulong current = 0;
                        var buffer = new byte[BUFFER_LENGTH];
                        var count = 0;
                        do
                        {
                            count = reader.Read(buffer, 0, BUFFER_LENGTH);
                            current += (ulong)count;
                            builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
                            updateProgress?.Invoke(current, total);
                        }
                        while (count > 0);
                    }

                    return builder.ToString();
                }
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }

        #endregion
        #region Download File

        /// <summary>
        /// Скачивание файла (GET).
        /// </summary>
        /// <param name="href">путь на диске.</param>
        /// <param name="currentPath">Файл локальный.  </param>
        /// <param name="updateProgress"> для передачи прогресса </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/get.html/.</remarks>
        public async Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null)
        {
            try
            {
                HttpWebRequest request = YandexHelper.RequestDownloadFile(accessToken, href);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    var length = response.ContentLength;
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
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion
        #region Delete file and directory

        /// <summary>
        /// Удаление файла или каталога по указанному пути.
        /// </summary>
        /// <param name="path">путь по которому надо удалить файл или каталок.</param>
        /// <returns>успех операции.</returns>
        /// <exception cref="FileNotFoundException" > ошибка </exception>
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
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion
        #region Load File

        /// <summary>
        /// Загрузить файл на сервер.
        /// </summary>
        /// <param name="href">Путь к файлу на диске. </param>
        /// <param name="fileName">Путь к файлу на компьюткрк.</param>
        /// <param name="progressChenge"> для передачи прогресса </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        public async Task<bool> LoadFileAsync(string href, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            try
            {
                // загрузка
                FileInfo fileInfo = new FileInfo(fileName);
                string diskName = YandexHelper.FileName(href, fileInfo.Name);
                HttpWebRequest request = YandexHelper.RequestLoadFile(accessToken, diskName);
                using (var reader = fileInfo.OpenRead())
                {
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

                // прием
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse httpResponse)
                    {
                        if (httpResponse.StatusCode == HttpStatusCode.Created) return true;
                        if (httpResponse.StatusCode == HttpStatusCode.InsufficientStorage) return false;
                        if (httpResponse.StatusCode == HttpStatusCode.Continue) return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }

            return false;
        }
        #endregion
        #region COPY TODO

        /// <summary>
        /// Копирование (COPY).
        /// </summary>
        /// <param name="originPath"> путь к оригинальному файлу </param>
        /// <param name="copyPath"> путь в который нужно скопировать </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html.</remarks>
        public Task<bool> CopyAsync(string originPath, string copyPath)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion
        #region MOVE TODO

        /// <summary>
        /// Перемещение и переименование (MOVE).
        /// </summary>
        /// <param name="originPath"> путь к оригинальному файлу </param>
        /// <param name="movePath"> путь в который надо переместить файл(или переименовать) </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">Время ожидания сервера вышло.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html.</remarks>
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
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion

        private Exception WebExceptionHandler(Exception exception)
        {
            if (exception is WebException web)
            {
                if (web.Status == WebExceptionStatus.Timeout)
                {
                    return new TimeoutException("Время ожидания сервера вышло.", web);
                }
                else if (web.Status == WebExceptionStatus.ProtocolError)
                {
                    if (web.Response is HttpWebResponse http)
                    {
                        if (http.StatusCode == HttpStatusCode.NotFound)
                        {
                            string message = $"Запрашиваемый файл или коталог отсутвует. uri ={http.ResponseUri}";
                            return new FileNotFoundException(message, web);
                        }

                        if (http.StatusCode == HttpStatusCode.Conflict)
                        {
                            string message = $"Запрашиваемый файл или коталог отсутвует. uri ={http.ResponseUri}";
                            return new FileNotFoundException(message, web);
                        }
                    }
                }
            }

            return exception;
        }
    }
}
