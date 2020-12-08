#define TABLE
//#define PLAIN
#undef PLAIN

using MRS.Bim.Tools;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MRS.DocumentManagement.Utils.Reports
{
    public static class ReportCreator
    {
#if PLAIN
        private static readonly string VERTICAL_ELEMENT = "VerticalElement";
#endif
        private static readonly string HORIZONTAL_ELEMENT = "HorizontalElement";
        private static readonly string TEXT = "Text";
        private static readonly string IMAGE = "Image";
        private static readonly string TABLE = "Table";
        private static readonly string ROW = "Row";
        private static readonly string CELL = "Cell";
        private static readonly string DEFAULT = "-";
        
        private static int count = 0;
        private static string reportId = "";
        private static ReportCount reportCount = new ReportCount();

        public static void Create(string projectName, params object[] element)
        {
            var date = DateTime.Now;

            if (string.IsNullOrEmpty(reportCount.ID))
            {
                reportCount.ID = date.ToString("ddMMyyyy");
                reportCount.Count = 0;
            }

            reportCount.Count++;

            reportId = $"{reportCount.ID}-{reportCount.Count}";


            var xml = new XElement("Report",
                    new XAttribute("project", projectName),
                    new XAttribute("number", reportId),
                    new XAttribute("date", date.ToShortDateString()));

#if TABLE
            XElement body = new XElement(TABLE, element.Select(GenerateXElement));
            xml.Add(body);
#endif
#if PLAIN

            xml.Add(element.Select(GenerateXElement));
#endif
            var path = Path.Combine(PathUtility.Instance.ProjectDirectory, $"Отчет {reportId}.docx");

            Thread.Sleep(10);
            var s = xml.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"");

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(BimEnvironment.Instance.StreamingAssetsPath, "Document Creator",
                            "DocumentCreator.exe"),
                    Arguments = "\"" + path + "\" \"" + s + "\""
                }
            };

            process.Start();
            process.WaitForExit();

            count = 0;

            Process.Start(path);

            //Task.Run(() =>
            //ConnectionHandler.Instance.Upload(ConnectionHandler.Instance.CurrentProject.ID, new DMFile
            //{
            //    Name = Path.GetFileName(path),
            //    Path = path
            //}));

        }


        private static XElement GenerateXElement(object obj)
        {
            switch (obj)
            {
                case ObjectiveToReportDto issue:
                    return GenerateXElement(issue);
                case ItemDto file:
                    return GenerateXElement(file);
                default:
                    return null;
            }
        }

#if PLAIN
        private static XElement GenerateXElement(Issue issue)
        {
            var result = new XElement(VERTICAL_ELEMENT,
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("ID: ", true),
                            TextElement(issue.ID.ToString())),
                    new XElement(HORIZONTAL_ELEMENT,
                            new XAttribute("align", "center"),
                            TextElement(issue.Name, true)),
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("Описание: ", true),
                            TextElement(issue.Description)),
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("Приложение: ", true)));
            result.Add(issue.Attachments.Select(GenerateXElement));
            result.Add(new XElement(HORIZONTAL_ELEMENT,
                    TextElement("Дата: ", true),
                    TextElement(issue.Detect.ToShortDateString())));
            return result;
        }
#endif

#if TABLE
        private static XElement GenerateXElement(ObjectiveToReportDto objective)
        {
            var result = new XElement(ROW,
                new XElement(CELL,
                    (objective.Items == null || objective.Items?.Count() < 1) ? 
                        new XElement[] { new XElement(HORIZONTAL_ELEMENT, TextElement(DEFAULT)) } :
                        objective.Items.Select(GenerateXElement)),
                new XElement(CELL,
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("ID: ", true),
                            TextElement($"{reportId}/{++count}")),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement("Время: ", true),
                             TextElement(objective.CreationDate.ToShortTimeString())),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement("Позиция: ", true),
                             TextElement(DEFAULT)),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement("Объект модели: ", true),
                             TextElement((objective.BimElements == null || objective.BimElements?.Count() < 1) ?
                                          DEFAULT : string.Join(", ", objective.BimElements))),
                    new XElement(HORIZONTAL_ELEMENT,
                             TextElement("Пользователь: ", true),
                             TextElement("Пользователь " + objective.Author)), //TODO: Authorization to db
                    new XElement(HORIZONTAL_ELEMENT,
                            TextElement("Описание: ", true),
                            TextElement(objective.Description))));

            return result;
        }
#endif
        private static XElement GenerateXElement(ItemDto file)
            => PathUtility.IsPicture(file.Path)
                    ? new XElement(HORIZONTAL_ELEMENT,
                            new XElement(IMAGE, file.Path))
                    : null;

        private static XElement TextElement(string text, bool isBold = false)
        {
            var items = new List<object>();
            if (isBold)
                items.Add(new XAttribute("style", "bold"));
            items.Add(text);
            return new XElement(TEXT, items.ToArray());
        }
    }
}