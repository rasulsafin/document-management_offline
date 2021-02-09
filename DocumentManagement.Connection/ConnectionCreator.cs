using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    /// <summary>
    /// TODO: Factory for connections. Make it static or?..
    /// </summary>
    public class ConnectionCreator
    {
        public IConnection GetConnection(ConnectionTypeDto type)
        {
            return type.Name switch
            {
                //"tdms" => new TdmsConnection(),
                //"bim360" => new Bim360Connection(),
                //"yandexdisk" => new YandexDiskConnection(),
                { } => null,
            };
        }
    }
}