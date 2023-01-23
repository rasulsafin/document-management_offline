using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ReportService : ServiceBase, IReportService
    {
        private static readonly string PATH = "Report";

        public ReportService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ObjectiveReportCreationResultDto> GenerateReport(ReportDto report, string path, int userID, string projectName)
            => await Connection.PostObjectJsonQueryAsync<ReportDto, ObjectiveReportCreationResultDto>($"{PATH}/create",
                $"path={{0}}&userID={{1}}&projectName={{2}}",
                new object[] { path, userID, projectName },
                report);
    }
}
