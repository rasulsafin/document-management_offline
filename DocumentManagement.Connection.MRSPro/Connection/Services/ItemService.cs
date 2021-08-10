using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ItemService : Service
    {
        public ItemService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        public async Task<FileInfo> GetAsync(string command, string fullFileName)
        {
            var directory = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await using var output = File.OpenWrite(fullFileName);
            await using var stream = await HttpConnection.GetResponseStreamAuthorizedAsync(
                    HttpMethod.Get,
                    command);
            await stream.CopyToAsync(output);
            return new FileInfo(fullFileName);
        }
    }
}
