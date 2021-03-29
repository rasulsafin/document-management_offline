using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Utility
{
    public class RequestProcessing
    {
        public static readonly Dictionary<string, (Task<IResult> task, double progress)> QUEQUE 
            = new Dictionary<string, (Task<IResult> task, double progress)>();

        public double GetProgress(string id)
        {
            if (QUEQUE.TryGetValue(id, out var job))
            {
                var result = job.progress;
                return result;
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public Task<IResult> GetResult(string id)
        {
            if (QUEQUE.TryGetValue(id, out var job))
            {
                if (job.task.IsCompleted)
                {
                    var result = job.task.Result;
                    QUEQUE.Remove(id);
                    return Task.FromResult(result);
                }
                else
                { return null; }
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }

        public void SetProgress(double value, string id)
        {
            if (QUEQUE.TryGetValue(id, out var job))
            {
                QUEQUE[id] = (job.task, value);
                return;
            }

            throw new ArgumentException($"The job {id} doesn't exist");
        }
    }
}
