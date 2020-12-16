using System;
using System.Collections.Generic;
using System.Xml;

namespace DocumentManagement.Connection.YandexDisk
{
    public class DiskElement
    {
        public string status;
        public string creationdate;
        public string displayname;
        public string getcontentlength;
        public string getlastmodified;
        public string resourcetype;
        public string href;
        public string getcontenttype;
        public string mulca_file_url;
        public string mulca_digest_url;
        private string file_url;

        public bool IsDirectory { get; private set; }

        #region Create
        internal static List<DiskElement> GetElements(XmlElement root)
        {
            var result = new List<DiskElement>();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:response")
                    {
                        DiskElement item = GetElement(element);
                        result.Add(item);
                    }
                    else
                        Console.WriteLine($"GetElements: Неизвестный тег [{element.Name}]");
                }
                else
                    Console.WriteLine($"GetElements: Неизвестный тип [{node.GetType().Name}]");
            }
            return result;
        }

        private static DiskElement GetElement(XmlElement root)
        {
            DiskElement result = new DiskElement();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:href") result.href = GetValueElement(element);
                    else if (element.Name == "d:propstat") GetPropstat(result, element);
                    else
                        Console.WriteLine($"GetElement: Неизвестный тег [{element.Name}]");
                }
                else
                    Console.WriteLine($"GetElement: Неизвестный тип [{node.GetType().Name}]");
            }
            return result;
        }

        private static string GetValueElement(XmlElement element)
        {
            return Uri.UnescapeDataString(element.InnerText);
        }

        private static void GetPropstat(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:status") result.status = GetValueElement(element);
                    else if (element.Name == "d:prop") GetProp(result, element);
                    else
                        Console.WriteLine($"GetPropstat: Неизвестный тег [{element.Name}]");
                }
                else
                    Console.WriteLine($"GetPropstat: Неизвестный тип [{node.GetType().Name}]");
            }
        }

        private static void GetProp(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:creationdate") result.creationdate = GetValueElement(element);
                    else if (element.Name == "d:displayname") result.displayname = GetValueElement(element);
                    else if (element.Name == "d:getcontentlength") result.getcontentlength = GetValueElement(element);
                    else if (element.Name == "d:getlastmodified") result.getlastmodified = GetValueElement(element);
                    else if (element.Name == "d:resourcetype") GetResourcetype(result, element);
                    else if (element.Name == "d:getcontenttype") result.getcontenttype = GetValueElement(element);
                    else if (element.Name == "mulca_file_url") result.mulca_file_url = GetValueElement(element);
                    else if (element.Name == "d:getetag") result.mulca_digest_url = GetValueElement(element);
                    else if (element.Name == "file_url") result.file_url = GetValueElement(element);
                    else if (element.Name == "mulca_digest_url") result.mulca_digest_url = GetValueElement(element);
                    else
                        Console.WriteLine($"GetProp: Неизвестный тег [{element.Name}]");
                }
                else
                    Console.WriteLine($"GetProp: Неизвестный тип [{node.GetType().Name}]");
            }
        }

        private static void GetResourcetype(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:collection") result.IsDirectory = true;
                    else
                        Console.WriteLine($"GetResourcetype: Неизвестный тег [{element.Name}]");
                }
                else if (node is XmlText text)
                    result.resourcetype = text.Value;
                else
                    Console.WriteLine($"GetResourcetype: Неизвестный тип [{node.GetType().Name}]");
            }
        }
        #endregion
    }
}
