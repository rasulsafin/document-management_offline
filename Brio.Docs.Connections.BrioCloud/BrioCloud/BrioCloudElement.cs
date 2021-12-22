using System;
using System.Collections.Generic;
using System.IO;
using Brio.Docs.External.Utils;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudElement : CloudElement
    {
        public string ETag { get; private set; }

        public static List<CloudElement> GetElements(IReadOnlyCollection<WebDavResource> collection, string uri)
        {
            var result = new List<CloudElement>();
            foreach (var element in collection)
            {
                if (Uri.UnescapeDataString(element.Uri) != uri)
                {
                    BrioCloudElement item = GetElement(element);
                    result.Add(item);
                }
            }

            return result;
        }

        private static BrioCloudElement GetElement(WebDavResource element)
        {
            var result = new BrioCloudElement
            {
                Href = Uri.UnescapeDataString(element.Uri),
                IsDirectory = element.IsCollection,
                DisplayName = Path.GetFileName(element.Uri.TrimEnd('/')),
                ContentType = element.ContentType,
                ETag = element.ETag,
            };

            result.CreationDate = element.CreationDate ?? default;
            result.ContentLength = (ulong?)element.ContentLength ?? default;
            result.LastModified = element.LastModifiedDate ?? default;

            return result;
        }
    }
}
