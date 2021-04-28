using MRS.DocumentManagement.Utility.Extensions;
using Serilog;

namespace MRS.DocumentManagement.Api.Extensions
{
    public static class DMApiLoggerExtensions
    {
        public static LoggerConfiguration DestructureByIgnoringSensitive(this LoggerConfiguration configuration)
        {
            return configuration.DestructureByIgnoringSensitiveDMInfo();
        }
    }
}
