using System.Collections.Generic;
using System.Drawing;
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
        private static readonly IReadOnlyDictionary<string, ImagePartType> IMAGE_PART_MAP
            = new Dictionary<string, ImagePartType>
        {
            { ".bmp", ImagePartType.Bmp },
            { ".emf", ImagePartType.Emf },
            { ".gif", ImagePartType.Gif },
            { ".ico", ImagePartType.Icon },
            { ".jpg", ImagePartType.Jpeg },
            { ".jpeg", ImagePartType.Jpeg },
            { ".pcx", ImagePartType.Pcx },
            { ".png", ImagePartType.Png },
            { ".svg", ImagePartType.Svg },
            { ".tiff", ImagePartType.Tiff },
            { ".wmf", ImagePartType.Wmf },
        };

        private static uint ids = 1U;
        private readonly int imageWidth = 600; // Fixed width

        public override void Read(XElement node, OpenXmlElement element)
        {
            var imagePath = node.Value;
            if (!File.Exists(imagePath))
                return;

            var extension = Path.GetExtension(imagePath);
            if (!IMAGE_PART_MAP.TryGetValue(extension, out var imageType))
                return;

            ImagePart imagePart = ReportCreator.MainPart.AddImagePart(imageType);

            var img = Image.FromFile(imagePath);

            double width = imageWidth;
            double height = width * ((double)img.Height / img.Width);

            img.Dispose();

            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            Drawing imageElement = GetImageElement(
                ReportCreator.MainPart.GetIdOfPart(imagePart),
                imagePath,
                Path.GetFileName(imagePath),
                width,
                height);

            element.Append(new Wordprocessing.Run(imageElement));
        }

        private static Drawing GetImageElement(string imagePartId, string fileName, string pictureName, double width, double height)
        {
            const double englishMetricUnitsPerInch = 914400;
            const double pixelsPerInch = 96;

            // calculate size in emu
            double emuWidth = width * englishMetricUnitsPerInch / pixelsPerInch;
            double emuHeight = height * englishMetricUnitsPerInch / pixelsPerInch;

            var element = new Drawing(
                new Inline(
                    new Extent { Cx = (Int64Value)emuWidth, Cy = (Int64Value)emuHeight },
                    new EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DocProperties { Id = (UInt32Value)ids++, Name = pictureName }, // IDs should be unique
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
