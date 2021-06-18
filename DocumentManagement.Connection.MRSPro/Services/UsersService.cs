using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class UsersService : Service
    {
        private static readonly string BASE_URL = "/user";

        public UsersService(MrsProHttpConnection connection)
          : base(connection) { }

        internal async Task<User> GetMe()
        {
            var users = await HttpConnection.Get<IEnumerable<User>>(BASE_URL);
            return users.FirstOrDefault();
        }
    }
}
