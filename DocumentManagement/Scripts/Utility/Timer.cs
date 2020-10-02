using System;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MethodSupportsCancellation

namespace MRS.Bim.Tools
{
    public class Timer
    {
        private CancellationTokenSource cancellation;

        public void SetTimer(float seconds, Action callback = null)
        {
            //Debug.Log("Timer starts");
            cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            Task.Run(async () =>
            {
                await Task.Delay((int) seconds * 1000);

                if (!token.IsCancellationRequested)
                    callback?.Invoke();
            });
        }

        public void StopTimer()
        {
            //Debug.Log("Timer stopped");
            cancellation.Cancel();
        }
    }
}