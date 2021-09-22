using Brio.Docs.Interface;
using System;
using System.Threading.Tasks;

namespace Brio.Docs.Connection.MrsPro.Converters
{
    internal class ExternalIdTypeIdConverter : IConverter<string, (string id, string type)>
    {
        public Task<(string id, string type)> Convert(string idPath)
        {
            if (string.IsNullOrEmpty(idPath))
                return Task.FromResult((string.Empty, string.Empty));

            var idParts = idPath.Split(Constants.ID_PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            var id = idParts[^1].Split(Constants.ID_SPLITTER);
            return Task.FromResult((id[0], id[1].ToLower()));
        }
    }
}
