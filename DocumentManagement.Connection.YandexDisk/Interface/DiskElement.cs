using System;
using System.Collections.Generic;
using System.Xml;

namespace MRS.DocumentManagement.Connection
{
    public class DiskElement
    {
        private string status;
        private string creationDate;
        private string displayName;
        private string contentLength;
        private string lastModifiedString;
        private DateTime lastModified;
        private string resourceType;
        private string href;
        private string contentType;
        private string mulcaFileUrl;
        private string mulcaDigestUrl;
        private string fileUrl;
        private string etag;

        public string DisplayName { get => displayName; }

        public DateTime LastModified { get => lastModified; }

        public string ContentLength { get => contentLength; set => contentLength = value; }

        public string ContentType { get => contentType; set => contentType = value; }

        public bool IsDirectory { get; private set; }

        public string Status { get => status; }

        public string CreationDate { get => creationDate; set => creationDate = value; }

        public string ResourceType { get => resourceType; set => resourceType = value; }

        public string Href { get => href; set => href = value; }

        public string MulcaFileUrl { get => mulcaFileUrl; set => mulcaFileUrl = value; }

        public string MulcaDigestUrl { get => mulcaDigestUrl; set => mulcaDigestUrl = value; }

        public string FileUrl { get => fileUrl; set => fileUrl = value; }

        public string Etag { get => etag; set => etag = value; }

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
                    {
                        Console.WriteLine($"GetElements: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    Console.WriteLine($"GetElements: Неизвестный тип [{node.GetType().Name}]");
                }
            }

            return result;
        }

        internal static DiskElement GetElement(XmlElement root)
        {
            DiskElement result = new DiskElement();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:href") result.href = GetValueElement(element);
                    else if (element.Name == "d:propstat") GetPropstat(result, element);
                    else if (element.Name == "d:response") result = GetElement(element);
                    else
                        Console.WriteLine($"GetElement: Неизвестный тег [{element.Name}]");
                }
                else
                {
                    Console.WriteLine($"GetElement: Неизвестный тип [{node.GetType().Name}]");
                }
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
                {
                    Console.WriteLine($"GetPropstat: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static void GetProp(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:creationdate") result.creationDate = GetValueElement(element);
                    else if (element.Name == "d:displayname") result.displayName = GetValueElement(element);
                    else if (element.Name == "d:getcontentlength") result.contentLength = GetValueElement(element);
                    else if (element.Name == "d:getlastmodified") SetLastModified(result, element);
                    else if (element.Name == "d:resourcetype") GetResourcetype(result, element);
                    else if (element.Name == "d:getcontenttype") result.contentType = GetValueElement(element);
                    else if (element.Name == "mulca_file_url") result.mulcaFileUrl = GetValueElement(element);
                    else if (element.Name == "d:getetag") result.etag = GetValueElement(element);
                    else if (element.Name == "file_url") result.fileUrl = GetValueElement(element);
                    else if (element.Name == "mulca_digest_url") result.mulcaDigestUrl = GetValueElement(element);
                    else
                        Console.WriteLine($"GetProp: Неизвестный тег [{element.Name}]");
                }
                else
                {
                    Console.WriteLine($"GetProp: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static void SetLastModified(DiskElement result, XmlElement element)
        {
            result.lastModifiedString = GetValueElement(element);
            result.lastModified = DateTime.Parse(result.lastModifiedString);
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
                {
                    result.resourceType = text.Value;
                }
                else
                {
                    Console.WriteLine($"GetResourcetype: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }
        #endregion
    }
}
