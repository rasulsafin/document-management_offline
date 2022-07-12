﻿using System.Xml.Linq;
using DocumentFormat.OpenXml;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal interface IElement
    {
        void Read(XElement node, OpenXmlElement element);

        void SetAttributes(XElement node, OpenXmlElement properties);
    }
}