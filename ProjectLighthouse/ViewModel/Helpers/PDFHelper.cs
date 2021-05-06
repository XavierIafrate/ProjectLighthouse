using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ProjectLighthouse.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class PDFHelper
    {

        public static void PrintOrder(LatheManufactureOrder order, ObservableCollection<LatheManufactureOrderItem> items)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string fileName = string.Format("{0}_{1:ddMMyyHHmmss}.pdf", order.Name, DateTime.Now);
            using (PdfDocument document = new PdfDocument())
            {
                //Debug
                var brush = new XSolidBrush(XColor.FromArgb(120, 255,0,0));
                var bluebrush = new XSolidBrush(XColor.FromArgb(120, 0, 0, 255));

                // Init
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                
                // Logo
                XImage image = XImage.FromFile(".\\Resources\\Lighthouse_dark.png");
                const double dy = 200;
                double width = image.PixelWidth * 72 / image.HorizontalResolution;
                double height = image.PixelHeight * 72 / image.HorizontalResolution;
                width /= 10;
                height /= 10;
                gfx.DrawRectangle(XPens.Black, brush, new XRect(page.Width / 2 - (width / 2), (dy - height) / 2, width, height));
                gfx.DrawImage(image, page.Width/2-(width/2), (dy - height) / 2, width, height);


                XFont font = new XFont("Tahoma", 30, XFontStyle.Bold, options);

                gfx.DrawRectangle(XPens.Transparent, brush, new XRect(page.Width / 2 - 100, 130, 200, 50));

                gfx.DrawString(order.Name, font, XBrushes.Black, new XRect(page.Width / 2 - 100, 130, 200, 50), XStringFormats.Center);
                font = new XFont("Tahoma", 20, XFontStyle.Bold, options);

                // PORef
                // CreatedAt
                // UrgentFlag
                // Start Date
                // Machine
                // Setter
                // Bar ID
                // # of Bars

                // Last Update
                // Updated By
                // Notes


                int y = (int)400;
                int i = (int)0;
                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);
                foreach (var item in items)
                {
                    XRect RowNumCol = new XRect(30, y, 70, 20);
                    XRect ProductNameCol = new XRect(100, y, 150, 20);

                    XRect QuantityRequiredCol = new XRect(250, y, 50, 20);
                    XRect DateRequiredCol = new XRect(300, y, 70, 20);
                    XRect TargetQuantityCol = new XRect(370, y, 50, 20);
                    XRect CycleTimeCol = new XRect(420, y, 50, 20);

                    //debug
                    gfx.DrawRectangle(XPens.Black, brush, RowNumCol);
                    gfx.DrawRectangle(XPens.Black, bluebrush, ProductNameCol);
                    gfx.DrawRectangle(XPens.Black, brush, QuantityRequiredCol);
                    gfx.DrawRectangle(XPens.Black, bluebrush, DateRequiredCol);
                    gfx.DrawRectangle(XPens.Black, brush, TargetQuantityCol);
                    gfx.DrawRectangle(XPens.Black, bluebrush, CycleTimeCol);

                    if (i == 0)
                    {

                    }
                    else 
                    { 
                    gfx.DrawString(string.Format("{0}.", i), font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                    gfx.DrawString(item.ProductName, font, XBrushes.Black, ProductNameCol, XStringFormats.CenterLeft);

                    gfx.DrawString(string.Format("{0:#,##0}", 10000), font, XBrushes.Black, QuantityRequiredCol, XStringFormats.CenterRight);
                    gfx.DrawString(string.Format("{0:dd/MM/yy}", item.DateRequired), font, XBrushes.Black, DateRequiredCol, XStringFormats.Center);
                    gfx.DrawString(string.Format("{0:#,##0}", item.TargetQuantity), font, XBrushes.Black, TargetQuantityCol, XStringFormats.CenterRight);
                    gfx.DrawString(string.Format("{0} s", item.CycleTime), font, XBrushes.Black, CycleTimeCol, XStringFormats.CenterRight);
                    }

                    y += 20;
                    i += 1;
                }

                document.Save(fileName);
                OpenWithDefaultProgram(fileName);
                MessageBox.Show("Success");
            }
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }
    }
}
