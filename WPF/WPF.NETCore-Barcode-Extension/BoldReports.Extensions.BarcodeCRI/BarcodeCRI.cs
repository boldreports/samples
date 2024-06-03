namespace BoldReports.Extensions.BarcodeCRI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using BoldReports.RDL.Data;
    using System.Threading;
    using BarcodeLib;
    using System.Windows.Controls;
    using Syncfusion.Pdf.Barcode;
    using BoldReports.Windows;

    /// <summary>
    /// The report processor first calls the GenerateReportItemDefinition method 
    /// and then calls the EvaluateReportItemInstance method to get the rendered 
    /// report item.
    /// </summary>
    public class BarcodeCustomReportItem : ICustomReportItem
    {
        bool displayText { get; set; }
        string barcodeType { get; set; }
        string barcodeValue { get; set; }
        int barcodeHeight { get; set; }
        int barcodeWidth { get; set; }
        string backColor { get; set; }

        public IReportLogger Logger { get; set; }

        #region ICustomReportItem Members

        public void GenerateReportItemDefinition(CustomReportItem cri)
        {
            cri.CreateCriImageDefinition();
        }

        public void EvaluateReportItemInstance(CustomReportItem cri)
        {
            Thread thread = new Thread(delegate ()
            {
                BoldReports.RDL.Data.Image polygonImage = (BoldReports.RDL.Data.Image)cri.GeneratedReportItem;
                polygonImage.ImageData = DrawImage(cri);
            }, (1024 * 1024 * 64));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #endregion

        private byte[] DrawImage(CustomReportItem customReportItem)
        {
            try
            {
                byte[] imageData = null;
                displayText = Convert.ToBoolean(LookupCustomProperty(customReportItem.CustomProperties, "DisplayBarcodeText"));
                barcodeType = LookupCustomProperty(customReportItem.CustomProperties, "BarcodeType").ToString().ToUpper();
                barcodeValue = (string)LookupCustomProperty(customReportItem.CustomProperties, "BarcodeValue");
                barcodeHeight = (Int32)customReportItem.Height.ToPixels();
                barcodeWidth = (Int32)customReportItem.Width.ToPixels();
                backColor = customReportItem.Style.BackgroundColor;

                if (barcodeType == "QRBARCODE")
                {
                    PdfQRBarcode barcodeControl = new PdfQRBarcode();
                    return GetBarcodeImage(barcodeControl);
                }
                else if (barcodeType == "PDF417")
                {
                    Pdf417Barcode barcodeControl = new Pdf417Barcode();
                    barcodeControl.ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Auto;
                    return GetBarcodeImage(barcodeControl);
                }
                else if (barcodeType == "DATAMATRIX")
                {
                    PdfDataMatrixBarcode barcodeControl = new PdfDataMatrixBarcode();
                    barcodeControl.Encoding = PdfDataMatrixEncoding.Auto;
                    return GetBarcodeImage(barcodeControl);
                }
                else
                {
                    TYPE type = TYPE.UNSPECIFIED;
                    switch (barcodeType)
                    {
                        case "UPCBARCODE": type = TYPE.UPCA; break;
                        case "EAN-13": type = TYPE.EAN13; break;
                        case "EAN-8": type = TYPE.EAN8; break;
                        case "CODE11": type = TYPE.CODE11; break;
                        case "CODE39": type = TYPE.CODE39; break;
                        case "CODE39EXTENDED": type = TYPE.CODE39Extended; break;
                        case "CODABAR": type = TYPE.Codabar; break;
                        case "CODE39 MOD 43": type = TYPE.CODE39_Mod43; break;
                        case "CODE93": type = TYPE.CODE93; break;
                        case "INTERLEAVED 2 OF 5": type = TYPE.Interleaved2of5; break;
                        case "STANDARD 2 OF 5": type = TYPE.Standard2of5; break;
                        case "CODE128": type = TYPE.CODE128; break;
                        case "CODE128A": type = TYPE.CODE128A; break;
                        case "CODE128B": type = TYPE.CODE128B; break;
                        case "CODE128C": type = TYPE.CODE128C; break;
                        case "PHARMACODE": type = TYPE.PHARMACODE; break;
                        default:
                            throw new Exception("Specified barcode type is not supported");
                    }

                    if (type != TYPE.UNSPECIFIED)
                    {
                        Barcode barcode = new Barcode();

                        if (Enum.IsDefined(typeof(TYPE), barcodeType))
                        {
                            barcode.EncodedType = (TYPE)Enum.Parse(typeof(TYPE), barcodeType, true);
                        }
                        if (customReportItem.Style != null && !string.IsNullOrEmpty(customReportItem.Style.BackgroundColor))
                        {
                            barcode.BackColor = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromInvariantString(backColor);
                        }
                        if (displayText)
                        {
                            barcode.IncludeLabel = true;
                        }

                        barcode.AlternateLabel = barcodeValue;
                        System.Drawing.Image barcodeimage = barcode.Encode(type, barcodeValue, barcodeWidth, barcodeHeight);
                        return GetImageByteData(barcodeimage);
                    }
                    return imageData;
                }
            }
            catch
            {
                return null;
            }
        }

        private byte[] GetBarcodeImage(PdfBidimensionalBarcode barcodeControl)
        {
            if (!string.IsNullOrEmpty(backColor))
            {
                barcodeControl.BackColor = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromInvariantString(backColor);
            }
            else
            {
                barcodeControl.BackColor = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromInvariantString("Transparent");
            }

            barcodeControl.Size = new System.Drawing.SizeF(barcodeWidth, barcodeHeight);
            barcodeControl.XDimension = 5;
            barcodeControl.Text = barcodeValue;

            System.Drawing.Image barcodeImage = barcodeControl.ToImage();
            return GetImageByteData(barcodeImage);
        }

        private byte[] GetImageByteData(System.Drawing.Image barcodeimage)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                barcodeimage.Save(stream, ImageFormat.Png);
                byte[] imageData = new byte[(int)stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(imageData, 0, (int)stream.Length);
                return imageData;
            }
        }

        private object LookupCustomProperty(RDL.DOM.CustomProperties customProperties, string name)
        {
            object customPropertyValue = bool.FalseString;

            if (customProperties == null || customProperties.Count == 0)
                return customPropertyValue;

            RDL.DOM.CustomProperty customProperty = (from cp in customProperties where cp.Name == name select cp).FirstOrDefault();

            if (customProperty != null && customProperty.Name == "BarcodeValue")
            {
                if ((!string.IsNullOrEmpty(customProperty.Value) && customProperty.Value.StartsWith("=")) || string.IsNullOrEmpty(customProperty.Value))
                {
                    customPropertyValue = "0000";
                }
                else
                {
                    customPropertyValue = customProperty.Value;
                }
            }
            else
            {
                if (customProperty != null && !string.IsNullOrEmpty(customProperty.Value))
                {
                    customPropertyValue = customProperty.Value;
                }
            }

            return customPropertyValue;
        }

        public string GetReportItemData(CustomReportItem cri)
        {
            try
            {
                byte[] imageData = null;
                Thread thread = new Thread(delegate()
                {
                    imageData = DrawImage(cri);
                }, (1024 * 1024 * 64));

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return Convert.ToBase64String(imageData);
            }
            catch
            {
                return null;
            }
        }
    }

    internal partial class ImageConversion : UserControl
    {
        Canvas InnerCanvas;

        public MemoryStream CovertToImage(Control innerControl)
        {
            try
            {
                this.InnerCanvas = new Canvas();
                this.Content = this.InnerCanvas;

                innerControl.Margin = new Thickness(0);
                InnerCanvas.Children.Add(innerControl);

                InnerCanvas.Width = innerControl.Width;
                InnerCanvas.Height = innerControl.Height;

                Canvas canvas = this.InnerCanvas;
                canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
                canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));

                int Height = ((int)(InnerCanvas.ActualHeight));
                int Width = ((int)(InnerCanvas.ActualWidth));
#if !MVC
                Height = (int)((Height * 300) / 96);
                Width = (int)((Width * 300) / 96);
#endif

                this.Height = InnerCanvas.Height;
                this.Width = InnerCanvas.Width;
                InnerCanvas.LayoutTransform = null;

                Size size = new Size(InnerCanvas.ActualWidth, InnerCanvas.ActualHeight);

                InnerCanvas.Background = Brushes.White;
                InnerCanvas.Arrange(new Rect(size));
                InnerCanvas.UpdateLayout();
#if MVC
                RenderTargetBitmap rtb = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Default);
#else

                RenderTargetBitmap rtb = new RenderTargetBitmap(Width, Height, 300, 300, PixelFormats.Default);
#endif
                rtb.Render(InnerCanvas);

                var Source = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(Source);
                return Source;
            }
            catch
            {
                return null;
            }
        }
    }
}
