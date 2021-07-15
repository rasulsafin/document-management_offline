using MRS.DocumentManagement.Connection.MrsPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class PlansService : Service
    {
        private static readonly string BASE_URL = "/plan";

        public PlansService(MrsProHttpConnection connection)
            : base(connection) { }

    }
}
