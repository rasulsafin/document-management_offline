using Brio.Docs.Connections.LementPro.Models;
using Destructurama;
using Serilog;

namespace Brio.Docs.Connections.LementPro
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
