﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// Interface for working with remote storage.
    /// </summary>
    public interface IConnectionStorage
    {
        /// <summary>
        /// Download files from remote storage.
        /// </summary>
        /// <param name="projectId">Id of the project to download from.</param>
        /// <param name="itemExternalDto">Items to download.</param>
        /// <param name="progress">Progress to be tracked.</param>
        /// <param name="token">CancellationToken for cancellation.</param>
        /// <returns>Download result.</returns>
        Task<bool> DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto, IProgress<double> progress, CancellationToken token);

        /// <summary>
        /// Deletes files from remote storage.
        /// </summary>
        /// <param name="projectId">Project id with items.</param>
        /// <param name="projectName">Project name with items.</param>
        /// <param name="itemExternalDtos">Items to delete.</param>
        /// <param name="progress">Progress to be tracked.</param>
        /// <returns>Deletion result.</returns>
        Task<bool> DeleteFiles(string projectId, string projectName, IEnumerable<ItemExternalDto> itemExternalDtos, IProgress<double> progress);
    }
}
