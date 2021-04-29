using Destructurama;
using MRS.DocumentManagement.Connection.LementPro.Models;
using Serilog;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public static class LPLoggerExtensions
    {
        public static LoggerConfiguration DestructureByIgnoringSensitiveDMInfo(this LoggerConfiguration configuration)
        {
            configuration = configuration.Destructure.ByIgnoringProperties<AuthorizationData>(x => x.Password);
            return configuration;
        }
    }
}
