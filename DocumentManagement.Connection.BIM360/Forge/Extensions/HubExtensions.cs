using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Extensions
{
    public static class HubExtensions
    {
        public static string GetAccountID(this Hub hub)
            => hub.ID.Remove(0, 2);

        public static bool IsEmea(this Hub hub)
            => hub.Attributes.Region == Region.Emea;
    }
}
