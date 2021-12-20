using System;
using System.Collections.Generic;
using System.IO;
using Brio.Docs.External.Utils;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudElement : CloudElement
    {
        public static List<BrioCloudElement> GetElements(IReadOnlyCollection<WebDavResource> collection)
        {
            var result = new List<BrioCloudElement>();
            foreach (var element in collection)
            {
                BrioCloudElement item = GetElement(element);
                result.Add(item);
            }

            return result;
        }

        private static BrioCloudElement GetElement(WebDavResource element)
        {
            var result = new BrioCloudElement();

            result.Href = Uri.UnescapeDataString(element.Uri);
            result.DisplayName = Path.GetFileName(result.Href);
            result.IsDirectory = element.IsCollection;

            return result;
        }
    }
}
