using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Microsoft.Extensions.Localization;

namespace Brio.Docs.Utility
{
    public class ReportHelper
    {
        private static readonly string HORIZONTAL_ELEMENT = "HorizontalElement";
        private static readonly string HEADING_ELEMENT = "HeadingElement";
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

        internal XDocument Convert(List<ObjectiveToReportDto> objectives, string path, string projectName, string reportId, DateTime date)
        {
            var xml = new XElement("Report",
                    new XAttribute("project", projectName),
                    new XAttribute("number", reportId),
                    new XAttribute("date", date.ToShortDateString()));

            var objectiveTypes = objectives.OrderBy(o => o.Status).GroupBy(o => o.Status);

            foreach (var objectiveType in objectiveTypes)
            {
                var heading = new XElement(HEADING_ELEMENT, HeadingElement($"{localizer["Status"]}: {StatusToString(objectiveType.Key)}"));
                xml.Add(heading);
                var body = new XElement(TABLE, objectiveType.Select(GenerateXElement));
                xml.Add(body);
            }

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

        private XElement GenerateXElement(ObjectiveToReportDto objective)
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

            var result = new XElement(ROW,
                new XElement(CELL, itemsElements),
                new XElement(CELL,
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("ID: ", true),
                            TextElement($"{objective.ID}")),
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement($"{localizer["Status"]}: ", true),
                            TextElement($"{StatusToString(objective.Status)}")),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement($"{localizer["Time"]}: ", true),
                             TextElement(objective.CreationDate.ToString("g"))),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement($"{localizer["Position"]}: ", true),
                             TextElement(locationTextElement)),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement($"{localizer["Model_Object"]}: ", true),
                             TextElement(bimElementsText)),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement($"{localizer["User"]}: ", true),
                             TextElement(objective.Author)),
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement($"{localizer["Title"]}: ", true),
                            TextElement(objective.Title)),
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement($"{localizer["Description"]}: ", true),
                            TextElement(objective.Description))));

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

        private XElement HeadingElement(string text)
        {
            var items = new List<object>();
            items.Add(new XAttribute("fontsize", "heading"));
            items.Add(new XAttribute("style", "bold"));
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
