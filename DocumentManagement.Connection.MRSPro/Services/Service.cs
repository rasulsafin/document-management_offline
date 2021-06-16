using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class Service
    {
        public Service(MrsProHttpConnection connection)
        {
            HttpConnection = connection;
        }

        protected MrsProHttpConnection HttpConnection { get; }

        protected string GetListAsString(IReadOnlyCollection<string> list)
        {
            StringBuilder str = new ();
            var count = list.Count - 1;

            for (int i = 0; i < count; i++)
                str.Append(list.ElementAt(i)).Append(',');

            str.Append(list.ElementAt(count));

            return str.ToString();
        }
    }
}
