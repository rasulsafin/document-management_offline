using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection
{
    public interface ICloudController
    {

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="nameDir"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<CloudElement> CreateDirAsync(string path, string nameDir);

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> DeleteAsync(string path);

        /// <summary>
        ///
        /// </summary>
        /// <param name="href"></param>
        /// <param name="currentPath"></param>
        /// <param name="updateProgress"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="updateProgress"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> GetContentAsync(string path, Action<ulong, ulong> updateProgress = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<CloudElement>> GetListAsync(string path = "/");

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<CloudElement> GetInfoAsync(string path = "/");

        /// <summary>
        ///
        /// </summary>
        /// <param name="href"></param>
        /// <param name="fileName"></param>
        /// <param name="progressChenge"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<CloudElement> LoadFileAsync(string href, string fileName, Action<ulong, ulong> progressChenge = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="progressChenge"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null);
    }
}
