using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface
{
    public interface ISynchronizer<T>
    {
        Task<T> Add(T obj);

        Task<T> Remove(T obj);

        Task<T> Update(T obj);
    }
}
