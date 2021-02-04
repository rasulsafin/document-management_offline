﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace MRS.DocumentManagement.Connection
{
    public class DiskElement
    {
        private string lastModifiedString;

        public string DisplayName { get; private set; }

        public DateTime LastModified { get; private set; }

        public string ContentLength { get; private set; }

        public string ContentType { get; private set; }

        public bool IsDirectory { get; private set; }

        public string Status { get; private set; }

        public string CreationDate { get; private set; }

        public string ResourceType { get; private set; }

        public string Href { get; private set; }

        public string MulcaFileUrl { get; private set; }

        public string MulcaDigestUrl { get; private set; }

        public string FileUrl { get; private set; }

        public string ETag { get; private set; }

        #region Create
        public static List<DiskElement> GetElements(XmlElement root)
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
                        throw new XmlException($"GetElements: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    throw new XmlException($"GetElements: Неизвестный тип [{node.GetType().Name}]");
                }
            }

            return result;
        }

        public static DiskElement GetElement(XmlElement root)
        {
            DiskElement result = new DiskElement();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:href":
                            result.Href = GetValueElement(element);
                            break;
                        case "d:propstat":
                            GetPropStatus(result, element);
                            break;
                        case "d:response":
                            result = GetElement(element);
                            break;
                        default:
                            throw new XmlException($"GetElement: Неизвестный тег [{element.Name}]");
                            break;
                    }
                }
                else
                {
                    throw new XmlException($"GetElement: Неизвестный тип [{node.GetType().Name}]");
                }
            }

            return result;
        }

        private static string GetValueElement(XmlElement element)
        {
            return Uri.UnescapeDataString(element.InnerText);
        }

        private static void GetPropStatus(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:status":
                            result.Status = GetValueElement(element);
                            break;
                        case "d:prop":
                            GetProp(result, element);
                            break;
                        default:
                            throw new XmlException($"GetPropStatus: Неизвестный тег [{element.Name}]");
                            break;
                    }
                }
                else
                {
                    throw new XmlException($"GetPropstat: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static void GetProp(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:creationdate":
                            result.CreationDate = GetValueElement(element);
                            break;
                        case "d:displayname":
                            result.DisplayName = GetValueElement(element);
                            break;
                        case "d:getcontentlength":
                            result.ContentLength = GetValueElement(element);
                            break;
                        case "d:getlastmodified":
                            SetLastModified(result, element);
                            break;
                        case "d:resourcetype":
                            GetResourceType(result, element);
                            break;
                        case "d:getcontenttype":
                            result.ContentType = GetValueElement(element);
                            break;
                        case "mulca_file_url":
                            result.MulcaFileUrl = GetValueElement(element);
                            break;
                        case "d:getetag":
                            result.ETag = GetValueElement(element);
                            break;
                        case "file_url":
                            result.FileUrl = GetValueElement(element);
                            break;
                        case "mulca_digest_url":
                            result.MulcaDigestUrl = GetValueElement(element);
                            break;
                        default:
                            throw new XmlException($"GetProp: Неизвестный тег [{element.Name}]");
                            break;
                    }
                }
                else
                {
                    throw new XmlException($"GetProp: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static void SetLastModified(DiskElement result, XmlElement element)
        {
            result.lastModifiedString = GetValueElement(element);
            result.LastModified = DateTime.Parse(result.lastModifiedString);
        }

        private static void GetResourceType(DiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:collection":
                            result.IsDirectory = true;
                            break;
                        default:
                            throw new XmlException($"GetResourcetype: Неизвестный тег [{element.Name}]");
                            break;
                    }
                }
                else if (node is XmlText text)
                {
                    result.ResourceType = text.Value;
                }
                else
                {
                    throw new XmlException($"GetResourcetype: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }
        #endregion
    }
}
