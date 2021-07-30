using System.Linq;
using Destructurama;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Serilog;

namespace MRS.DocumentManagement.Utility.Extensions
{
    public static class DMLoggerExtensions
    {
        public static LoggerConfiguration DestructureByIgnoringSensitiveDMInfo(this LoggerConfiguration configuration)
        {
            configuration = configuration.Destructure.ByIgnoringProperties<User>(
                x => x.PasswordHash,
                x => x.PasswordSalt);
            configuration = configuration.Destructure.ByIgnoringProperties<AuthFieldValue>(x => x.Value);
            configuration =
                configuration.Destructure.ByIgnoringProperties<ConnectionTypeExternalDto>(x => x.AppProperties);
            configuration =
                configuration.Destructure.ByIgnoringProperties<ConnectionInfoExternalDto>(x => x.AuthFieldValues);
            configuration = configuration.DestructureByIgnoringSensitiveExternalInfo();
            return configuration;
        }

        private static LoggerConfiguration DestructureByIgnoringSensitiveExternalInfo(this LoggerConfiguration configuration)
            => ConnectionCreator.GetLoggerMethods()
               .Aggregate(
                    configuration,
                    (aggregated, method) => (LoggerConfiguration)method.Invoke(null, new object[] { aggregated }));
    }
}