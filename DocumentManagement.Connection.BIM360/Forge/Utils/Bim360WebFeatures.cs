using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public static class Bim360WebFeatures
    {
        private static readonly string AUTODESK_WEBSITE = "https://autodesk.com/";

        public static async Task<bool> CanPingAutodesk()
            => await WebFeatures.RemoteUrlExistsAsync(AUTODESK_WEBSITE);
    }
}
