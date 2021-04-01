using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Utility
{
    public class RequestQueueService : IRequestQueueService, IRequestService
    {
        private static readonly Dictionary<string, Request> QUEUE
            = new Dictionary<string, Request>();

        public Task<double> GetProgress(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                var result = job.Progress;
                return Task.FromResult(result);
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public Task Cancel(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Src.Cancel();
                QUEUE.Remove(id);
                return Task.CompletedTask;
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public Task<RequestResult> GetResult(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                if (job.Task.IsCompleted)
                {
                    var result = job.Task.Result;
                    QUEUE.Remove(id);
                    return Task.FromResult(result);
                }
                else
                {
                    // TODO: throw exception
                    return null;
                }
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public void AddRequest(string id, Task<RequestResult> task, CancellationTokenSource src)
            => QUEUE.Add(id, new Request(task, src));

        public void SetProgress(double value, string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Progress = value;
                return;
            }
        }
    }
}
