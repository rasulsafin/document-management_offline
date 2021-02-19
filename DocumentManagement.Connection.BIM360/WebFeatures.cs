﻿using System.Net;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public static class WebFeatures
    {
        public static async Task<bool> RemoteUrlExistsAsync(string url)
        {
            try
            {
                // Creating the HttpWebRequest
                var request = WebRequest.Create(url);

                // Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";

                // Getting the Web Response.
                var response = (HttpWebResponse)await request.GetResponseAsync();

                var urlExist = response.StatusCode == HttpStatusCode.OK;

                // Returns TRUE if the Status code == 200
                response.Close();
                return urlExist;
            }
            catch
            {
                // Any exception will returns false.
                return false;
            }
        }
    }
}
