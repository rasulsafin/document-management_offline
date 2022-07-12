using System.IO;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DrawingWordprocessing = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Path = System.IO.Path;
using Picture = DocumentFormat.OpenXml.Drawing.Pictures.Picture;
using Pictures = DocumentFormat.OpenXml.Drawing.Pictures;
using Wordprocessing = DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class ImageElement : AElement
    {
        private static readonly double WIDTH = 680;
        private static readonly double HEIGHT = 400;
        private static readonly double MAP_WIDTH = 680;
        private static readonly double MAP_HEIGHT = 400;

        private static readonly string MAP_EXTENSION = ".map.png";

        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!File.Exists(node.Value))
                return;

            double width = WIDTH;
            double height = HEIGHT;

            if (node.Value.EndsWith(MAP_EXTENSION))
            {
                width = MAP_WIDTH;
                height = MAP_HEIGHT;
            }

            ImagePart imagePart = ReportCreator.MainPart.AddImagePart(ImagePartType.Png);

            using (FileStream stream = new FileStream(node.Value, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            Drawing imageElement = GetImageElement(
            ReportCreator.MainPart.GetIdOfPart(imagePart),
            node.Value,
            Path.GetFileName(node.Value),
            width,
            height);

            element.Append(new Wordprocessing.Run(imageElement));
        }

        private static Drawing GetImageElement(string imagePartId, string fileName, string pictureName, double width, double height)
        {
            double englishMetricUnitsPerInch = 914400;
            double pixelsPerInch = 96;

            //calculate size in emu
            double emuWidth = width * englishMetricUnitsPerInch / pixelsPerInch;
            double emuHeight = height * englishMetricUnitsPerInch / pixelsPerInch;

            var element = new Drawing(
                new Inline(
                    new Extent { Cx = (Int64Value)emuWidth, Cy = (Int64Value)emuHeight },
                    new EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DocProperties { Id = (UInt32Value)1U, Name = pictureName },
                    new DrawingWordprocessing.NonVisualGraphicFrameDrawingProperties(
                    new GraphicFrameLocks { NoChangeAspect = true }),
                    new Graphic(
                        new GraphicData(
                            new Picture(
                                new Pictures.NonVisualPictureProperties(
                                    new Pictures.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = fileName },
                                    new Pictures.NonVisualPictureDrawingProperties()),
                                new Pictures.BlipFill(
                                    new Blip(
                                        new BlipExtensionList(
                                            new BlipExtension { Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" }))
                                    {
                                        Embed = imagePartId,
                                        CompressionState = BlipCompressionValues.Print,
                                    },
                                    new Stretch(new FillRectangle())),
                                new Pictures.ShapeProperties(
                                    new Transform2D(
                                        new Offset { X = 0L, Y = 0L },
                                        new Extents { Cx = (Int64Value)emuWidth, Cy = (Int64Value)emuHeight }),
                                    new PresetGeometry(
                                        new AdjustValueList())
                                    { Preset = ShapeTypeValues.Rectangle })))
                        {
                            Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture",
                        }))
                {
                    DistanceFromTop = (UInt32Value)0U,
                    DistanceFromBottom = (UInt32Value)0U,
                    DistanceFromLeft = (UInt32Value)0U,
                    DistanceFromRight = (UInt32Value)0U,
                    EditId = "50D07946",
                });
            return element;
        }
    }
}
