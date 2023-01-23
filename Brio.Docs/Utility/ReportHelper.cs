using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Brio.Docs.Client.Dtos;
using Microsoft.Extensions.Localization;

namespace Brio.Docs.Utility
{
    public class ReportHelper
    {
        private static readonly string HORIZONTAL_ELEMENT = "HorizontalElement";
        private static readonly string IMAGE = "Image";
        private static readonly string TABLE = "Table";
        private static readonly string ROW = "Row";
        private static readonly string CELL = "Cell";

        private static readonly string[] PICTURES_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

        private readonly IStringLocalizer<ReportLocalization> localizer;

        public ReportHelper(IStringLocalizer<ReportLocalization> localizer)
        {
            this.localizer = localizer;
        }

        internal XDocument Convert(ReportDto report, List<ObjectiveToReportDto> objectives, string reportId, DateTime date)
        {
            var xml = new XElement("Report",
                    new XAttribute("number", reportId),
                    new XAttribute("location", report.Location),
                    new XAttribute("date", date.ToString("d", Thread.CurrentThread.CurrentUICulture)),
                    new XAttribute("reviewer_position", report.ReviewerPosition),
                    new XAttribute("reviewer_company", report.ReviewerCompany),
                    new XAttribute("reviewer_name", report.ReviewerName),
                    new XAttribute("responsible_position", report.ResponsiblePosition),
                    new XAttribute("responsible_company", report.ResponsibleCompany),
                    new XAttribute("responsible_name", report.ResponsibleName),
                    new XAttribute("object_name", report.ObjectName),
                    new XAttribute("address", report.Address),
                    new XAttribute("verification_subject", report.VerificationSubject));

            var objectiveTypes = objectives.OrderBy(o => o.Status).GroupBy(o => o.Status);

            foreach (var objectiveType in objectiveTypes)
            {
                var objectiveTypeList = objectiveType.ToList();
                for (int i = 0; i < objectiveTypeList.Count(); i++)
                {
                    var objective = objectiveTypeList[i];

                    var objectiveIndex = i + 1;
                    var body = GenerateObjectiveElement(objective, objectiveIndex);
                    xml.Add(body);
                }
            }

            foreach (var objectiveType in objectiveTypes)
            {
                var body = new XElement(TABLE, objectiveType.Select(GenerateItemsElement));
                xml.Add(body);
            }

            return new XDocument(xml);
        }

        private XElement GenerateObjectiveElement(ObjectiveToReportDto objective, int index)
        {
            var dynamicFields = objective.DynamicFields.Where(x => x.Key.StartsWith("model_metadata")).ToList();
            var documentation = new StringBuilder();
            for (int i = 0; i < dynamicFields.Count; i++)
            {
                var field = dynamicFields[i];
                if (i != dynamicFields.Count - 1)
                    documentation.AppendLine($"{field.Name}:\n{field.Value};");
                else
                    documentation.AppendLine($"{field.Name}:\n{field.Value}");
            }

            var result = new XElement("Objective",
            new XAttribute("objective_index", index),
            new XAttribute("objective_description", $"{objective.Title}\n{objective.Description}"),
            new XAttribute("objective_documentation", documentation),
            new XAttribute("objective_date", objective.DueDate.ToString("g")));

            return result;
        }

        private XElement GenerateItemsElement(ObjectiveToReportDto objective)
        {
            if (objective.Items == null || !objective.Items.Any())
                return null;

            var itemsElements = objective.Items.Select(GenerateXElement);

            var result = new XElement(ROW, new XElement(CELL, itemsElements));
            return result;
        }

        private XElement GenerateXElement(ItemDto file)
            => PICTURES_EXTENSIONS.Contains(Path.GetExtension(file.RelativePath).ToLower())
                    ? new XElement(HORIZONTAL_ELEMENT,
                            new XElement(IMAGE, file.RelativePath))
                    : null;
    }
}
