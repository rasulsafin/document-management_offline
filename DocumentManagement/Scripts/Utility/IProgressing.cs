using System.Threading;

namespace MRS.Bim.Tools
{
    public interface IProgressing
    {
        void AddLayer(int i);
        ProgressType ProgressType { get; set; }
        int LayersCount { get; }
        bool IsCanceled { get; }
        CancellationTokenSource CancellationTokenSource { get; }
        void Set();
        void Set(float f);
        void Cancel();
        void CloseLayer();
        void Clear();
    }
}