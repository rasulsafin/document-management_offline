using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Utility
{
    public class RequestQueueService : IRequestQueueService
    {
        public static readonly Dictionary<string, (Task<RequestResult> task, double progress, CancellationTokenSource src)> QUEUE
            = new Dictionary<string, (Task<RequestResult> task, double progress, CancellationTokenSource src)>();

        public Task<double> GetProgress(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                var result = job.progress;
                return Task.FromResult(result);
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public Task Cancel(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.src.Cancel();
                QUEUE.Remove(id);
                return Task.CompletedTask;
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public Task<RequestResult> GetResult(string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                if (job.task.IsCompleted)
                {
                    var result = job.task.Result;
                    QUEUE.Remove(id);
                    return Task.FromResult(result);
                }
                else
                { return null; }
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        internal void SetProgress(double value, string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                QUEUE[id] = (job.task, value, job.src);
                return;
            }

           // throw new ArgumentException($"The job {id} doesn't exist");
        }
    }
}
