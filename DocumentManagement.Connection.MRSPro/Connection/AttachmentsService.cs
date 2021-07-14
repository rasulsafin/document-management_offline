using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class AttachmentsService : Service
    {
        private static readonly string BASE_URL = "/attachment";

        public AttachmentsService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<Attachment> TryPost(Attachment attachment)
        {
            try
            {
                var result = await HttpConnection.PostJson<Attachment>(BASE_URL, attachment);
                   // new Attachment() { OriginalName = "image.png", ParentId = "60ed826800fac340ae7049fe", ParentType = "task" }
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
