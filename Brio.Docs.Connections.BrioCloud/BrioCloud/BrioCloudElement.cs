using System;
using System.Collections.Generic;
using System.IO;
using Brio.Docs.External.Utils;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudElement : CloudElement
    {
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
            var result = new BrioCloudElement();

            result.Href = Uri.UnescapeDataString(element.Uri);
            result.IsDirectory = element.IsCollection;
            result.DisplayName = Path.GetFileName(result.Href.TrimEnd('/'));

            return result;
        }
    }
}
