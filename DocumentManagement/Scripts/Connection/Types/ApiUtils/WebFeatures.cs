using System.Net;
using System.Threading.Tasks;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public static class WebFeatures
    {
        public static async Task<bool> RemoteUrlExistsAsync(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(url);
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                var response = (HttpWebResponse) await request.GetResponseAsync();
                //Returns TRUE if the Status code == 200
                response.Close();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
    }
}