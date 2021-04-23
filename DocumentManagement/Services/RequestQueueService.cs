using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class RequestQueueService : IRequestQueueService, IRequestService
    {
        private static readonly Dictionary<string, Request> QUEUE
            = new Dictionary<string, Request>();

        private readonly ILogger<RequestQueueService> logger;

        public RequestQueueService(ILogger<RequestQueueService> logger)
            => this.logger = logger;

        public Task<double> GetProgress(string id)
        {
            logger.LogTrace("GetProgress {@LongRequestID}", id);
            if (QUEUE.TryGetValue(id, out var job))
            {
                var result = job.Progress;
                return Task.FromResult(result);
            }

            throw new ArgumentNullException($"The job {id} doesn't exist");
        }

        public Task Cancel(string id)
        {
            logger.LogTrace("Cancel started for id = {@LongRequestID}", id);
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Src.Cancel();
                logger.LogInformation("Operation {@LongRequestID} canceled", id);
                QUEUE.Remove(id);
                return Task.CompletedTask;
            }

            throw new ArgumentNullException($"The job {id} doesn't exist");
        }

        public Task<RequestResult> GetResult(string id)
        {
            logger.LogTrace("GetResult started for id = {@LongRequestID}", id);
            if (QUEUE.TryGetValue(id, out var job))
            {
                if (job.Task.IsCompleted)
                {
                    var result = job.Task.Result;
                    logger.LogDebug("Result of the operation {@LongRequestID}: {@Result}", id, result);
                    QUEUE.Remove(id);
                    return Task.FromResult(result);
                }
                else
                {
                    logger.LogWarning("Trying to get result of the incomplete operation {@LongRequestID}", id);
                    throw new InvalidOperationException($"The job {id} is not finished yet");
                }
            }

            throw new ArgumentNullException($"The job {id} doesn't exist");
        }

        public void AddRequest(string id, Task<RequestResult> task, CancellationTokenSource src)
        {
            logger.LogTrace("AddRequest {@LongRequestID}", id);
            QUEUE.Add(id, new Request(task, src));
        }

        public void SetProgress(double value, string id)
        {
            logger.LogTrace("Progress {@LongRequestID} : {@Progress}", id, value);
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Progress = value;
                return;
            }
        }
    }
}
