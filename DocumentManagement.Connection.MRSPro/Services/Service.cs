using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class Service
    {
        public Service(MrsProHttpConnection connection)
        {
            HttpConnection = connection;
        }

        protected MrsProHttpConnection HttpConnection { get; }
    }
}
