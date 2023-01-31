using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brio.Docs.Reports.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharpDocx;

namespace Brio.Docs.Reports
{
    public class ReportGenerator
    {
        private static readonly string REPORTS_RES_FOLDER = @"ReportResources";
        private static readonly string REPORTS_MANIFEST = @"reports.json";

        private readonly Dictionary<string, ReportInfo> reports = new Dictionary<string, ReportInfo>();
        private readonly string reportResourcesFolder;
        private readonly ILogger logger;

        public ReportGenerator(ILogger<ReportGenerator> logger)
        {
            this.logger = logger;
            var currentExecutablePath = Assembly.GetEntryAssembly().Location;
            var currentExecutableFolder = Path.GetDirectoryName(currentExecutablePath);
            reportResourcesFolder = Path.Combine(currentExecutableFolder, REPORTS_RES_FOLDER);
            var reportsManifestPath = Path.Combine(reportResourcesFolder, REPORTS_MANIFEST);

            LoadReportsManifest(reportsManifestPath);
        }

        public IReadOnlyCollection<ReportInfo> AvailableReports => reports.Values;

        public static bool IsSupportedImageExtension(string filePath)
            => SharpImage.ImageInfo.GetType(filePath) != SharpImage.ImageInfo.Type.Unknown;

        public bool TryGetReportInfo(string reportTypeId, out ReportInfo info)
            => reports.TryGetValue(reportTypeId, out info);

        public void Generate(string reportTypeId, string outFilePath, ReportModel vm)
        {
            if (!reports.TryGetValue(reportTypeId, out var info))
                throw new ArgumentException("Can not find report template by ID: {ID}", reportTypeId);

            var templateFilePath = info.TemplateFilePath;
            File.Delete(outFilePath);

            var doc = DocumentFactory.Create(templateFilePath, vm);
            doc.Generate(outFilePath);
        }

        private void LoadReportsManifest(string path)
        {
            var info = ReadReportsManifest(path);
            if (info.Reports == null)
                return;

            foreach (var report in info.Reports)
            {
                if (IsReportInfoValid(report))
                {
                    var templatePath = Path.Combine(reportResourcesFolder, report.TemplateFilePath);
                    if (!File.Exists(templatePath))
                    {
                        logger.LogWarning("Template file in report {ID} does not exist: {Path}", report.ID, templatePath);
                        continue;
                    }

                    report.TemplateFilePath = templatePath;

                    reports.Add(report.ID, report);
                }
            }
        }

        private ReportsInfo ReadReportsManifest(string path)
        {
            if (!File.Exists(path))
                logger.LogWarning("File not found: {Path}", path);

            try
            {
                var content = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ReportsInfo>(content);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read {Path}: {Message}", REPORTS_MANIFEST, ex.Message);
                return new ReportsInfo();
            }
        }

        private bool IsReportInfoValid(ReportInfo report)
        {
            if (string.IsNullOrEmpty(report.ID))
            {
                logger.LogWarning($"Report type ID should not be empty");
                return false;
            }

            if (reports.ContainsKey(report.ID))
            {
                logger.LogWarning("Duplicate report type ID: {ID}", report.ID);
                return false;
            }

            if (string.IsNullOrEmpty(report.Name))
            {
                logger.LogWarning("Report type {ID} has no Name", report.ID);
                return false;
            }

            if (string.IsNullOrEmpty(report.TemplateFilePath))
            {
                logger.LogWarning("Template file path is not specified for report type {ID}", report.ID);
                return false;
            }

            return true;
        }

        public class ReportsInfo
        {
            public List<ReportInfo> Reports { get; set; } = new List<ReportInfo>();
        }

        public class ReportInfo
        {
            public string ID { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TemplateFilePath { get; set; }

            public List<string> Fields { get; set; } = new List<string>();
        }
    }
}
