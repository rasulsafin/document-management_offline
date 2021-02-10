﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskController : ICloudController
    {
        private string accessToken;

        public YandexDiskController(string accessToken)
        {
            this.accessToken = accessToken;
        }

        #region PROPFIND

        /// <summary>
        /// Returns a list of items.
        /// </summary>
        /// <param name="path">The path to the folder</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="FileNotFoundException"> File Not Found Exception </exception>
        /// <exception cref="TimeoutException" > Timeout Exception </exception>
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
                List<YandexDiskElement> items = YandexDiskElement.GetElements(xml.DocumentElement);
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
                YandexDiskElement item = YandexDiskElement.GetElement(xml.DocumentElement);
                return item;
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }

        #endregion
        #region Create Directory
        public async Task<DiskElement> CreateDirAsync(string path, string nameDir)
        {
            try
            {
                string newPath = PathManager.DirectoryName(path, nameDir);
                HttpWebRequest request = YandexHelper.RequestCreateDir(accessToken, newPath);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse http)
                    {
                        if (http.StatusCode == HttpStatusCode.Created)
                        {
                            return new YandexDiskElement() { };
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw WebExceptionHandler(ex);
            }
        }
        #endregion
        #region Content
        /// <summary>
        /// Write the content to a file and upload it.
        /// </summary>
        /// <param name="path">The path to the file on the disk. </param>
        /// <param name="content">Content</param>
        /// <param name="progressChenge"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null)
        {
            try
            {
                HttpWebRequest request = YandexHelper.RequestLoadFile(accessToken, path);
                using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(content)))
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

                using (WebResponse response = await request.GetResponseAsync())
                {
                    if (response is HttpWebResponse httpResponse)
                    {
                        switch (httpResponse.StatusCode)
                        {
                            case HttpStatusCode.Created:
                                return true;
                            case HttpStatusCode.InsufficientStorage:
                                return false;
                            case HttpStatusCode.Continue:
                                return false;
                        }
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
        /// Downloads the file and returns its contents.
        /// </summary>
        /// <param name="path">The path to the file on the disk. </param>
        /// <param name="updateProgress"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="DirectoryNotFoundException">Directory Not Found Exception</exception>
        /// <exception cref="FileNotFoundException">File Not Found Exception</exception>
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
        /// Downloading a file (GET).
        /// </summary>
        /// <param name="href">The path to the file on the disk. </param>
        /// <param name="currentPath">The path to the file on the computer.</param>
        /// <param name="updateProgress"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/get.html/.</remarks>
        /// <exception cref="DirectoryNotFoundException">Directory Not Found Exception</exception>
        /// <exception cref="FileNotFoundException">File Not Found Exception</exception>
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
        /// Deleting a file or directory at the specified path.
        /// </summary>
        /// <param name="path">the path to delete the file or gurney.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="FileNotFoundException" >No file needed</exception>
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
        /// Load File
        /// </summary>
        /// <param name="href">The path to the file on the disk. </param>
        /// <param name="fileName">The path to the file on the computer.</param>
        /// <param name="progressChenge"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
        public async Task<bool> LoadFileAsync(string href, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                string diskName = PathManager.FileName(href, fileInfo.Name);
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
        /// (COPY).
        /// </summary>
        /// <param name="originPath"> path to the original file </param>
        /// <param name="copyPath"> the way in which you need to copy </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
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
        /// (MOVE).
        /// </summary>
        /// <param name="originPath"> path to the original file </param>
        /// <param name="movePath"> the way in which it is necessary to move the file(or rename) </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
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
                    return new TimeoutException("The server timeout has expired.", web);
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
