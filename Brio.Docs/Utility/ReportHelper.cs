using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Microsoft.Extensions.Localization;

namespace Brio.Docs.Utility
{
    public class ReportHelper
    {
        private static readonly string HORIZONTAL_ELEMENT = "HorizontalElement";
        private static readonly string TEXT = "Text";
        private static readonly string IMAGE = "Image";
        private static readonly string TABLE = "Table";
        private static readonly string ROW = "Row";
        private static readonly string CELL = "Cell";
        private static readonly string DEFAULT = "-";

        private static readonly string[] PICTURES_EXTENSIONS = { ".png", ".jpg" };

        private readonly IStringLocalizer<ReportLocalization> localizer;

        public ReportHelper(IStringLocalizer<ReportLocalization> localizer)
        {
            this.localizer = localizer;
        }

        internal XDocument Convert(ReportDto report, List<ObjectiveToReportDto> objectives, string path, string projectName, string reportId, DateTime date)
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

        internal XDocument CreateFooter()
        {
            var xml = new XElement("Footer",
                    new XAttribute("created_with", localizer["Created_With"]));

            return new XDocument(xml);
        }

        internal XDocument CreateHeader()
        {
            var xml = new XElement("Header",
                    new XAttribute("company_fullname", localizer["Company_Fullname"]),
                    new XAttribute("company_shortname", localizer["Company_Shortname"]),
                    new XAttribute("company_type", localizer["Company_Type"]),
                    new XAttribute("company_requisites", localizer["Company_Requisites"]),
                    new XAttribute("company_address", localizer["Company_Address"]),
                    new XAttribute("company_id", localizer["Company_ID"]));

            return new XDocument(xml);
        }

        private string StatusToString(ObjectiveStatus status)
        {
            switch (status)
            {
                case ObjectiveStatus.Undefined:
                    return localizer["Status_Undefined"];
                case ObjectiveStatus.Open:
                    return localizer["Status_Open"];
                case ObjectiveStatus.InProgress:
                    return localizer["Status_InProgress"];
                case ObjectiveStatus.Ready:
                    return localizer["Status_Ready"];
                case ObjectiveStatus.Late:
                    return localizer["Status_Late"];
                case ObjectiveStatus.Closed:
                    return localizer["Status_Closed"];
                default:
                    return "-";
            }
        }

        private XElement GenerateObjectiveElement(ObjectiveToReportDto objective, int index)
        {
            var itemsElements = (objective.Items == null || !objective.Items.Any())
               ? new XElement[] { new XElement(HORIZONTAL_ELEMENT, TextElement(DEFAULT)) }
               : objective.Items.Select(GenerateXElement);

            var bimElementsText = (objective.BimElements == null || !objective.BimElements.Any())
                ? DEFAULT
                : string.Join(", ", objective.BimElements.Select(x => x.ElementName));

            var locationTextElement = (objective.Location == null || objective.Location?.Position == null)
                ? DEFAULT
                : string.Join("; ", objective.Location.Position);

            var result = new XElement("Objective",
            new XAttribute("objective_index", index),
            new XAttribute("objective_description", $"{objective.Title}\n{objective.Description}"),
            new XAttribute("objective_elements", bimElementsText),
            new XAttribute("objective_status", $"{StatusToString(objective.Status)}"),
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

        private XElement TextElement(string text, bool isBold = false)
        {
            var items = new List<object>();
            items.Add(new XAttribute("fontsize", "regular"));
            if (isBold)
            {
                items.Add(new XAttribute("style", "bold"));
            }

            items.Add(text);
            return new XElement(TEXT, items.ToArray());
        }

        private XElement GenerateXElement(ItemDto file)
            => PICTURES_EXTENSIONS.Contains(Path.GetExtension(file.RelativePath).ToLower())
                    ? new XElement(HORIZONTAL_ELEMENT,
                            new XElement(IMAGE, file.RelativePath))
                    : null;
    }
}
