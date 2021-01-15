using System;
using System.Collections.Generic;
using System.Xml;

namespace MRS.DocumentManagement.Connection
{
    public class DiskElement
    {


        private string status;
        private string creationdate;
        private string displayname;
        private string getcontentlength;
        private string getlastmodified;
        private DateTime lastmodified;
        private string resourcetype;
        private string href;
        private string getcontenttype;
        private string mulca_file_url;
        private string mulca_digest_url;
        private string file_url;
        private string getetag;

        public string DisplayName { get => displayname;}
        public DateTime LastModified { get => lastmodified;}
        public string ContentLength { get => getcontentlength; set => getcontentlength = value; }
        public string ContentType { get => getcontenttype; set => getcontenttype = value; }
        public bool IsDirectory { get; private set; }
        public string Status { get => status;  }
        public string Creationdate { get => creationdate; set => creationdate = value; }
        public string Resourcetype { get => resourcetype; set => resourcetype = value; }
        public string Href { get => href; set => href = value; }
        public string Mulca_file_url { get => mulca_file_url; set => mulca_file_url = value; }
        public string Mulca_digest_url { get => mulca_digest_url; set => mulca_digest_url = value; }
        public string File_url { get => file_url; set => file_url = value; }
        public string Getetag { get => getetag; set => getetag = value; }

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
                    else if (element.Name == "d:getlastmodified") SetLastModified(result, element);
                    else if (element.Name == "d:resourcetype") GetResourcetype(result, element);
                    else if (element.Name == "d:getcontenttype") result.getcontenttype = GetValueElement(element);
                    else if (element.Name == "mulca_file_url") result.mulca_file_url = GetValueElement(element);
                    else if (element.Name == "d:getetag") result.getetag = GetValueElement(element);
                    else if (element.Name == "file_url") result.file_url = GetValueElement(element);
                    else if (element.Name == "mulca_digest_url") result.mulca_digest_url = GetValueElement(element);
                    else
                        Console.WriteLine($"GetProp: Неизвестный тег [{element.Name}]");
                }
                else
                    Console.WriteLine($"GetProp: Неизвестный тип [{node.GetType().Name}]");
            }
        }

        private static void SetLastModified(DiskElement result, XmlElement element)
        {
            result.getlastmodified = GetValueElement(element);
            result.lastmodified = DateTime.Parse(result.getlastmodified);
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
