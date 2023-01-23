using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Brio.Docs.Utils.ReportCreator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ReportService : IReportService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IStringLocalizer<ReportLocalization> reportLocalizer;
        private readonly ILogger<ReportService> logger;
        private readonly ReportHelper reportHelper;

        public ReportService(
            DMContext context,
            IMapper mapper,
            IStringLocalizer<ReportLocalization> reportLocalizer,
            ILogger<ReportService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.reportLocalizer = reportLocalizer;
            this.logger = logger;

            reportHelper = new ReportHelper(reportLocalizer);
        }

        public Task<IEnumerable<AvailableReportTypeDto>> GetAvailableReportTypes()
        {
            throw new NotImplementedException();
        }

        public async Task<ObjectiveReportCreationResultDto> GenerateReport(ReportDto report, string path, int userID, string projectName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation(
                "GenerateReport started for user {UserId} with path = {Path}, projectName = {ProjectName} objectiveIds: {@ObjectiveIDs}",
                userID,
                path,
                projectName,
                report.Objectives);
            try
            {
                if (report.Objectives == null)
                    throw new ArgumentValidationException("Cannot create report without objectives");

                int count = 0;
                DateTime date = DateTime.Now.Date;

                var reportCount = await context.ReportCounts.FindAsync(userID);
                if (reportCount != null)
                {
                    if (reportCount.Date == date)
                        count = reportCount.Count;
                }
                else
                {
                    reportCount = new ReportCount() { UserID = userID, Count = count, Date = date };
                    await context.AddAsync(reportCount);
                }

                reportCount.Count = ++count;
                reportCount.Date = date;
                logger.LogDebug("Report Count updating: {@ReportCount}", reportCount);
                await context.SaveChangesAsync();

                string reportID = $"{date:yyyyMMdd}-{count}";

                List<ObjectiveToReportDto> objectives = new List<ObjectiveToReportDto>();
                var objNum = 1;
                foreach (var objectiveId in report.Objectives)
                {
                    var objective = await GetOrThrowAsync(objectiveId);
                    var objectiveToReport = mapper.Map<ObjectiveToReportDto>(objective);
                    objectiveToReport.ID = $"{reportID}/{objNum++}";

                    foreach (var item in objectiveToReport.Items)
                    {
                        var newName = Path.Combine(path, item.RelativePath.TrimStart('\\'));
                        item.RelativePath = newName;
                    }

                    objectives.Add(objectiveToReport);
                }

                logger.LogDebug("Objectives for report: {@Objectives}", objectives);
                var reportDir = Path.Combine(path, "Reports");
                Directory.CreateDirectory(reportDir);
                var reportName = reportLocalizer["Report"];
                path = Path.Combine(reportDir, $"{reportName} {reportID}.docx");
                var xmlDoc = reportHelper.Convert(report, objectives, reportID, date);
                logger.LogDebug("XML created: {@XDocument}", xmlDoc);

                ReportCreator reportCreator = new ReportCreator();
                reportCreator.CreateReport(xmlDoc, path);
                logger.LogInformation("Report created ({Path})", path);

                return new ObjectiveReportCreationResultDto()
                {
                    ReportPath = path,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't create report");
                if (ex is ArgumentValidationException || ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<Objective> GetOrThrowAsync(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started for objective {ID}", objectiveID);
            var dbObjective = await context.Objectives
                .Unsynchronized()
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.Location)
                     .ThenInclude(x => x.Item)
                .Include(x => x.DynamicFields)
                     .ThenInclude(x => x.ChildrenDynamicFields)
                .Include(x => x.Items)
                     .ThenInclude(x => x.Item)
                .Include(x => x.BimElements)
                     .ThenInclude(x => x.BimElement)
                .FindOrThrowAsync(x => x.ID, (int)objectiveID);

            logger.LogDebug("Found objective: {@DBObjective}", dbObjective);

            return dbObjective;
        }
    }
}
