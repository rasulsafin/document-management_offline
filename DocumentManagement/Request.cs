using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Utility
{
    public class Request
    {
        public Request(Task<RequestResult> task, CancellationTokenSource src)
        {
            Task = task;
            Src = src;
        }

        public Task<RequestResult> Task { get; set; }

        public double Progress { get; set; } = 0.0;

        public CancellationTokenSource Src { get; set; }
    }
}
