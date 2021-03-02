using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Synchronizers
{
    public class Bim360ItemsSynchronizer
    {
        private readonly ItemsService itemsService;

        public Bim360ItemsSynchronizer(ForgeConnection connection)
        {
            itemsService = new ItemsService(connection);
        }

        internal async Task<Item> PostItem(ItemExternalDto item, Folder folder)
        {
            // Replicate steps 5-7 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
            throw new NotImplementedException();
        }
    }
}
