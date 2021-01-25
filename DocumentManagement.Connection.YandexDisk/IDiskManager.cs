using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection
{
    public interface IDiskManager
    {
        Task<T> Pull<T>(string id);
        Task<bool> Push<T>(T @object, string id);
    }
}