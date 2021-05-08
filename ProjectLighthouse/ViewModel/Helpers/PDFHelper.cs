using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class PDFHelper
    {
        // *****************************************
        //
        // I wrote this two months after brain surgery, writing this was hands down more painful overall
        //
        // *****************************************

        private static string LMO_PDF_OUTPUTDIR = "H:\\Production\\Documents\\Works Orders";

        private static string Address = "Automotion Components\nAlexia House\nGlenmore Business Park\nChichester, UK\nP019 7BJ";

        public static void PrintOrder(LatheManufactureOrder order, ObservableCollection<LatheManufactureOrderItem> items)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (PdfDocument document = new PdfDocument())
            {
                #region Init parameters
                //Debug
                var brush = new XSolidBrush(XColor.FromArgb(120, 255,0,0));
                var bluebrush = new XSolidBrush(XColor.FromArgb(120, 0, 0, 255));

                // Init
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter formatter = new XTextFormatter(gfx);
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                #endregion

                #region Title
                // Logo
                string network = "H:\\Production\\Documents\\Works Orders\\Lighthouse\\Lighthouse_dark.png";
                string local = "C:\\Users\\xavie\\Desktop\\Lighthouse_dark.png";

                XImage logo = XImage.FromFile(Directory.Exists(network) ? network : local);
                const double dy = 150;
                double width = logo.PixelWidth * 72 / logo.HorizontalResolution;
                double height = logo.PixelHeight * 72 / logo.HorizontalResolution;
                width /= 10;
                height /= 10;
                XRect logoRect = new XRect(page.Width / 2 - (width / 2), (dy - height) / 2, width, height);
                //gfx.DrawRectangle(XPens.Black, brush, logoRect);
                gfx.DrawImage(logo, logoRect);


                // Title
                XFont font = new XFont("Tahoma", 35, XFontStyle.Bold, options);
                XRect titleRect = new XRect(page.Width / 2 - 150, 100, 300, 40);
                //gfx.DrawRectangle(XPens.Transparent, brush, titleRect);
                gfx.DrawString(order.Name, font, XBrushes.Black, titleRect, XStringFormats.Center);

                //subtitle
                font = new XFont("Tahoma", 20, XFontStyle.Bold, options);
                titleRect.Y += titleRect.Height;
                //gfx.DrawRectangle(XPens.Black, brush, titleRect);
                gfx.DrawString("MANUFACTURE ORDER", font, XBrushes.Black, titleRect, XStringFormats.Center);
                #endregion

                #region Metadata
                double y = (double)200; // for iterating down the page
                double col_1_x_parameter = page.Width.Value * ((double)1/(double)10); // two col arrangement
                double col_1_x_value = page.Width.Value * ((double)3 / (double)10);
                double col_2_x_parameter = page.Width.Value * ((double)5 / (double)10);
                double col_2_x_value = page.Width.Value * ((double)7 / (double)10);

                XFont parameterFont = new XFont("Tahoma", 10, XFontStyle.Bold, options);
                XFont valueFont = new XFont("Tahoma", 10, XFontStyle.Regular, options);

                XRect parameterRect = new XRect(col_1_x_parameter, y, col_1_x_value - col_1_x_parameter, 18);
                XRect valueRect = new XRect(col_1_x_value, y, col_2_x_parameter - col_1_x_value, 18);

                gfx.DrawString("Purchase Order #", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.POReference, valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Start Date", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(string.Format("{0:ddd dd/MM/yy}", order.StartDate), valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Assigned Machine", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.AllocatedMachine, valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Assigned Setter", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.AllocatedSetter, valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                //Second Column
                y = (double)200;
                parameterRect = new XRect(col_2_x_parameter, y, col_2_x_value - col_2_x_parameter, 18);
                valueRect = new XRect(col_2_x_value, y, page.Width.Value*0.9 - col_2_x_value, 18);

                gfx.DrawString("Date Created", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(string.Format("{0:ddd dd/MM/yy HH:mm}", order.CreatedAt), valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Last Updated", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(string.Format("{0:ddd dd/MM/yy HH:mm}", order.ModifiedAt), valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Updated By", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.ModifiedBy, valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                y += valueRect.Height;
                parameterRect.Y = y;
                valueRect.Y = y;

                gfx.DrawString("Program Ready?", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.HasProgram ? "Yes" : "No", valueFont, order.HasProgram ? XBrushes.Black : XBrushes.Red, valueRect, XStringFormats.CenterLeft);
                //gfx.DrawRectangle(XPens.Transparent, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Transparent, bluebrush, valueRect);

                #endregion

                #region Order Details
                XRect subtitleRect = new XRect(page.Width / 2 - 150, 290, 300, 25);
                font = new XFont("Tahoma", 16, XFontStyle.Bold, options);
                //gfx.DrawRectangle(XPens.Transparent, brush, subtitleRect);
                gfx.DrawString("ORDER DETAILS", font, XBrushes.Black, subtitleRect, XStringFormats.Center);

                y = 320;
                int i = (int)1;
                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);

                XRect RowNumCol = new XRect(0, y, 30, 20);
                XRect ProductNameCol = new XRect(0, y, 110, 20);
                XRect QuantityRequiredCol = new XRect(0, y, 90, 20);
                XRect DateRequiredCol = new XRect(0, y, 90, 20);
                XRect TargetQuantityCol = new XRect(0, y, 90, 20);
                XRect CycleTimeCol = new XRect(0, y, 90, 20);

                double offset = (page.Width - (RowNumCol.Width + ProductNameCol.Width + QuantityRequiredCol.Width + DateRequiredCol.Width + TargetQuantityCol.Width + CycleTimeCol.Width)) / 2;

                RowNumCol.X = offset;
                ProductNameCol.X = RowNumCol.X + RowNumCol.Width;
                QuantityRequiredCol.X = ProductNameCol.X + ProductNameCol.Width;
                DateRequiredCol.X = QuantityRequiredCol.X + QuantityRequiredCol.Width;
                TargetQuantityCol.X = DateRequiredCol.X + DateRequiredCol.Width;
                CycleTimeCol.X = TargetQuantityCol.X + TargetQuantityCol.Width;

                gfx.DrawString("#", font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                gfx.DrawString("Product", font, XBrushes.Black, ProductNameCol, XStringFormats.CenterLeft);
                gfx.DrawString("Quantity Req.", font, XBrushes.Black, QuantityRequiredCol, XStringFormats.Center);
                gfx.DrawString("Date Req.", font, XBrushes.Black, DateRequiredCol, XStringFormats.Center);
                gfx.DrawString("Target Qty.", font, XBrushes.Black, TargetQuantityCol, XStringFormats.Center);
                gfx.DrawString("Cycle Time", font, XBrushes.Black, CycleTimeCol, XStringFormats.Center);
                
                y += 20;

                XPen stroke = new XPen(XColors.Black, 2);
                gfx.DrawLine(stroke, offset, y, page.Width - offset, y);
                y += 3;

                int totalQtyReq = 0;
                int totalQtyTar = 0;

                int reqTime = 0;
                int totTime = 0;

                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);

                foreach (var item in items)
                {
                    RowNumCol.Y = y;
                    ProductNameCol.Y = y;
                    QuantityRequiredCol.Y = y;
                    DateRequiredCol.Y =y;
                    TargetQuantityCol.Y = y;
                    CycleTimeCol.Y = y;

                    //debug
                    //gfx.DrawRectangle(XPens.Black, brush, RowNumCol);
                    //gfx.DrawRectangle(XPens.Black, bluebrush, ProductNameCol);
                    //gfx.DrawRectangle(XPens.Black, brush, QuantityRequiredCol);
                    //gfx.DrawRectangle(XPens.Black, bluebrush, DateRequiredCol);
                    //gfx.DrawRectangle(XPens.Black, brush, TargetQuantityCol);
                    //gfx.DrawRectangle(XPens.Black, bluebrush, CycleTimeCol);

                    gfx.DrawString(string.Format("{0}.", i), font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                    gfx.DrawString(item.ProductName, font, XBrushes.Black, ProductNameCol, XStringFormats.CenterLeft);
                    gfx.DrawString(intToQuantity(item.RequiredQuantity), font, XBrushes.Black, QuantityRequiredCol, XStringFormats.Center);
                    gfx.DrawString(DateToDateStamp(item.DateRequired), font, XBrushes.Black, DateRequiredCol, XStringFormats.Center);
                    gfx.DrawString(intToQuantity(item.TargetQuantity), font, XBrushes.Black, TargetQuantityCol, XStringFormats.Center);
                    gfx.DrawString(intToCycleTime(item.CycleTime), font, XBrushes.Black, CycleTimeCol, XStringFormats.CenterRight);

                    totalQtyReq += item.RequiredQuantity;
                    totalQtyTar += item.TargetQuantity;

                    reqTime += item.RequiredQuantity * item.CycleTime;
                    totTime += item.TargetQuantity * item.CycleTime;

                    y += 25;
                    i += 1;
                }

                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
                
                RowNumCol.Y = y;
                ProductNameCol.Y = y;
                QuantityRequiredCol.Y = y;
                DateRequiredCol.Y = y;
                TargetQuantityCol.Y = y;
                CycleTimeCol.Y = y;

                //debug
                //gfx.DrawRectangle(XPens.Black, brush, RowNumCol);
                //gfx.DrawRectangle(XPens.Black, bluebrush, ProductNameCol);
                //gfx.DrawRectangle(XPens.Black, brush, QuantityRequiredCol);
                //gfx.DrawRectangle(XPens.Black, bluebrush, DateRequiredCol);
                //gfx.DrawRectangle(XPens.Black, brush, TargetQuantityCol);
                //gfx.DrawRectangle(XPens.Black, bluebrush, CycleTimeCol);

                gfx.DrawLine(stroke, offset, y, page.Width - offset, y);
                gfx.DrawString("", font, XBrushes.DarkBlue, RowNumCol, XStringFormats.Center);
                gfx.DrawString("", font, XBrushes.DarkBlue, ProductNameCol, XStringFormats.CenterLeft);
                gfx.DrawString(intToQuantity(totalQtyReq), font, XBrushes.DarkBlue, QuantityRequiredCol, XStringFormats.Center);
                gfx.DrawString("", font, XBrushes.DarkBlue, DateRequiredCol, XStringFormats.Center);
                gfx.DrawString(intToQuantity(totalQtyTar), font, XBrushes.DarkBlue, TargetQuantityCol, XStringFormats.Center);
                gfx.DrawString("", font, XBrushes.DarkBlue, CycleTimeCol, XStringFormats.CenterRight);

                #endregion

                #region Resources
                y += 25;

                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
                XRect resourcesSubtitle = new XRect(offset, y, page.Width - 2 * offset, 20);
                //gfx.DrawRectangle(XPens.Black, brush, resourcesSubtitle);
                gfx.DrawString("RESOURCES", font, XBrushes.Black, resourcesSubtitle, XStringFormats.CenterLeft);

                y += 18;
                parameterRect.Y = y;
                parameterRect.X = col_1_x_parameter;
                valueRect.Y = y;
                valueRect.X = col_1_x_value;

                //gfx.DrawRectangle(XPens.Black, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Black, bluebrush, valueRect);
                gfx.DrawString("Bar ID", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.BarID, valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);

                y += 18;

                parameterRect.Y = y;
                valueRect.Y = y;

                //gfx.DrawRectangle(XPens.Black, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Black, bluebrush, valueRect);
                gfx.DrawString("Estimated # Bars", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(order.NumberOfBars.ToString(), valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);

                y += 18;

                parameterRect.Y = y;
                valueRect.Y = y;

                //gfx.DrawRectangle(XPens.Black, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Black, bluebrush, valueRect);
                gfx.DrawString("Time for required", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(String.Format("{0}d {1}h {2}m", TimeSpan.FromSeconds(reqTime).Days, TimeSpan.FromSeconds(reqTime).Hours, TimeSpan.FromSeconds(reqTime).Minutes),
                    valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                y += 18;

                parameterRect.Y = y;
                valueRect.Y = y;

                //gfx.DrawRectangle(XPens.Black, brush, parameterRect);
                //gfx.DrawRectangle(XPens.Black, bluebrush, valueRect);
                gfx.DrawString("Time for balance", parameterFont, XBrushes.Black, parameterRect, XStringFormats.CenterLeft);
                gfx.DrawString(String.Format("{0}d {1}h {2}m", TimeSpan.FromSeconds(totTime).Days, TimeSpan.FromSeconds(totTime).Hours, TimeSpan.FromSeconds(totTime).Minutes), 
                    valueFont, XBrushes.Black, valueRect, XStringFormats.CenterLeft);
                #endregion

                #region Notes, Footer
                y += 25;

                XRect notesRect = new XRect(offset, y, page.Width - 2 * offset, 20);

                y += 20;

                if (y + offset + 20 < page.Height.Value)
                {
                    gfx.DrawString("NOTES", font, XBrushes.Black, notesRect, XStringFormats.TopLeft);
                    font = new XFont("Tahoma", 10, XFontStyle.Regular, options);
                    notesRect = new XRect(offset, y, page.Width - 2 * offset, page.Height.Value - y - offset);
                    formatter.DrawString(String.IsNullOrWhiteSpace(order.Notes) ? "***None***" : order.Notes, font, XBrushes.Black, notesRect, XStringFormats.TopLeft);
                }

                XRect footer = new XRect(offset, page.Height - offset, page.Width - 2 * offset, 20);
                font = new XFont("Tahoma", 8, XFontStyle.Regular, options);
                gfx.DrawString(string.Format("Generated in Lighthouse by {0} at {1:dd/MM/yy HH:mm}", App.currentUser.GetFullName(), DateTime.Now), font, XBrushes.Black, footer, XStringFormats.BottomLeft);
                #endregion


                string fileName = string.Format("{0}_{1:ddMMyy_HHmm}.pdf", order.Name, DateTime.Now);
                string path = Directory.Exists(LMO_PDF_OUTPUTDIR) ? Path.Join(LMO_PDF_OUTPUTDIR, fileName) : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                SavePDF(document, path, true);
            }
        }

        public static void PrintDeliveryNote(DeliveryNote deliveryNote, List<DeliveryItem> deliveryItems)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (PdfDocument document = new PdfDocument())
            {
                #region Init parameters
                //Debug
                var brush = new XSolidBrush(XColor.FromArgb(120, 255, 0, 0));
                var bluebrush = new XSolidBrush(XColor.FromArgb(120, 0, 0, 255));

                // Init
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter formatter = new XTextFormatter(gfx);
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                #endregion

                #region Title
                // Logo
                string network = "H:\\Production\\Documents\\Works Orders\\Lighthouse\\Lighthouse_dark.png";
                string local = "C:\\Users\\xavie\\Desktop\\Lighthouse_dark.png";
                
                XImage logo = XImage.FromFile(Directory.Exists(network) ? network : local);
                const double dy = 150;
                double width = logo.PixelWidth * 72 / logo.HorizontalResolution;
                double height = logo.PixelHeight * 72 / logo.HorizontalResolution;
                width /= 10;
                height /= 10;
                XRect logoRect = new XRect(page.Width / 2 - (width / 2), (dy - height) / 2, width, height);
                gfx.DrawImage(logo, logoRect);


                // Title
                XFont font = new XFont("Tahoma", 35, XFontStyle.Bold, options);
                XRect titleRect = new XRect(page.Width / 2 - 150, 100, 300, 40);
                gfx.DrawString(deliveryNote.Name, font, XBrushes.Black, titleRect, XStringFormats.Center);

                //subtitle
                font = new XFont("Tahoma", 20, XFontStyle.Bold, options);
                titleRect.Y += titleRect.Height;
                gfx.DrawString("DELIVERY NOTE", font, XBrushes.Black, titleRect, XStringFormats.Center);
                #endregion

                #region Metadata
                // Manufacturer
                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
                gfx.DrawString("MANUFACTURER", 
                    font, XBrushes.Black, 
                    new XRect(100, 190, 150, 20), 
                    XStringFormats.CenterLeft);
                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);
                formatter.DrawString(Address, font, XBrushes.Black, new XRect(100, 210, 150, 90));

                // Shipping Information
                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
                gfx.DrawString("SHIPPING", 
                    font, XBrushes.Black, 
                    new XRect(page.Width-270, 190, 150, 20), 
                    XStringFormats.CenterLeft);
                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);
                gfx.DrawString(string.Format("Shipped by: {0}", deliveryNote.DeliveredBy), 
                    font, XBrushes.Black, 
                    new XRect(page.Width-270, 190+17, 150, 20), 
                    XStringFormats.CenterLeft);
                gfx.DrawString(string.Format("Shipped at: {0:dd/MM/yyyy HH:mm}", deliveryNote.DeliveryDate), 
                    font, XBrushes.Black, 
                    new XRect(page.Width-270, 190+34, 150, 20), 
                    XStringFormats.CenterLeft);
                #endregion

                #region Order Details
                XRect subtitleRect = new XRect(page.Width / 2 - 150, 290, 300, 25);
                font = new XFont("Tahoma", 16, XFontStyle.Bold, options);
                gfx.DrawString("DELIVERY DETAILS", font, XBrushes.Black, subtitleRect, XStringFormats.Center);

                int y = (int)320;
                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);

                XRect RowNumCol = new XRect(0, y, 30, 20);
                XRect PurchaseRefCol = new XRect(0, y, 100, 20);
                XRect ProductCol = new XRect(0, y, 110, 20);
                XRect ThisDelCol = new XRect(0, y, 120, 20);
                XRect ToFollowCol = new XRect(0, y, 110, 20);

                double offset = (page.Width - (RowNumCol.Width + PurchaseRefCol.Width + ProductCol.Width + ThisDelCol.Width + ToFollowCol.Width)) / 2;

                RowNumCol.X = offset;
                PurchaseRefCol.X = RowNumCol.X + RowNumCol.Width;
                ProductCol.X = PurchaseRefCol.X + PurchaseRefCol.Width;
                ThisDelCol.X = ProductCol.X + ProductCol.Width;
                ToFollowCol.X = ThisDelCol.X + ThisDelCol.Width;

                gfx.DrawString("#", font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                gfx.DrawString("Purchase Order", font, XBrushes.Black, PurchaseRefCol, XStringFormats.CenterLeft);
                gfx.DrawString("Product", font, XBrushes.Black, ProductCol, XStringFormats.Center);
                gfx.DrawString("Qty. This Delivery", font, XBrushes.Black, ThisDelCol, XStringFormats.Center);
                gfx.DrawString("Qty. To Follow", font, XBrushes.Black, ToFollowCol, XStringFormats.Center);

                y += 20;

                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);
                XPen stroke = new XPen(XColors.Black, 2);
                gfx.DrawLine(stroke, offset, y, page.Width - offset, y);

                y += 3;
                int i = 1;

                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);

                // Init barcode
                BarCode barcode = new Code3of9Standard("EMPTY", new XSize(PurchaseRefCol.Width + ProductCol.Width, ProductCol.Height*0.8));
                barcode.StartChar = '*';
                barcode.EndChar = '*';
                
                foreach (var deliveryItem in deliveryItems)
                {

                    if (i % 2 == 0)
                        gfx.DrawRectangle(XPens.Transparent, new XSolidBrush(XColor.FromArgb(26, 8, 89, 152)), new XRect(offset, y, page.Width - 2*offset, 40));
                    
                    RowNumCol.Y = y;
                    PurchaseRefCol.Y = y;
                    ProductCol.Y = y;
                    ThisDelCol.Y = y;
                    ToFollowCol.Y = y;
                    
                    gfx.DrawString(string.Format("{0}.", i), font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                    gfx.DrawString(deliveryItem.PurchaseOrderReference, font, XBrushes.Black, PurchaseRefCol, XStringFormats.CenterLeft);
                    gfx.DrawString(deliveryItem.Product, font, XBrushes.Black, ProductCol, XStringFormats.Center);
                    gfx.DrawString(string.Format("{0:#,##0} pcs", deliveryItem.QuantityThisDelivery), font, XBrushes.Black, ThisDelCol, XStringFormats.Center);
                    gfx.DrawString(string.Format("{0:#,##0} pcs", deliveryItem.QuantityToFollow), font, XBrushes.Black, ToFollowCol, XStringFormats.Center);

                    barcode.Text = deliveryItem.Product.ToUpper();
                    gfx.DrawBarCode(barcode, XBrushes.Black, new XPoint(PurchaseRefCol.X, y+20));

                    i += 1;
                    y += 40;
                }

                y += 3;
                font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
                gfx.DrawLine(stroke, offset, y, page.Width - offset, y);
                gfx.DrawString("*** END ***", font, XBrushes.Black, new XRect(offset, y, page.Width - 2 * offset, 15), XStringFormats.BottomCenter);
                #endregion

                #region Footer

                // Signing area
                font = new XFont("Tahoma", 12, XFontStyle.Regular, options);
                stroke = new XPen(XColors.Black, 1);
                y = (int)page.Height - (int)offset - 20;
                gfx.DrawLine(stroke, offset, y, page.Width / 2 , y);
                gfx.DrawString("Received by", font, XBrushes.Black, new XRect(offset, y, 100, 20), XStringFormats.CenterLeft);
                gfx.DrawLine(stroke, page.Width / 2 + offset, y, page.Width-offset , y);
                gfx.DrawString("Date received", font, XBrushes.Black, new XRect(page.Width/2+offset, y, 100, 20), XStringFormats.CenterLeft);

                // Print stamp
                font = new XFont("Tahoma", 8, XFontStyle.Regular, options);
                XRect footer = new XRect(offset, page.Height - offset, page.Width - 2 * offset, 20);
                gfx.DrawString(string.Format("Generated in Lighthouse by {0} at {1:dd/MM/yy HH:mm}", "Xavier Iafrate", DateTime.Now), font, XBrushes.Black, footer, XStringFormats.BottomLeft);

                #endregion


                string fileName = string.Format("{0}_{1:ddMMyy_HHmm}.pdf", deliveryNote.Name, DateTime.Now);
                string path = Directory.Exists(LMO_PDF_OUTPUTDIR) ? Path.Join(LMO_PDF_OUTPUTDIR, fileName) : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                SavePDF(document, path, true);
            }
        }

        public static void SavePDF(PdfDocument document, string path, bool open_after)
        {
            try
            {
                document.Save(path);
                MessageBox.Show(String.Format("Saved to {0}", path), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                if (open_after)
                    OpenWithDefaultProgram(path);
            }
            catch (Exception e) // No connection to server with adapters installed
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Helper functions

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        public static string intToQuantity(int qty)
        {
            return qty == 0 ? "" : string.Format("{0:#,##0}", qty);
        }

        public static string DateToDateStamp(DateTime date_input)
        {
            return date_input == DateTime.MinValue ? "" : string.Format("{0:dd/MM/yy}", date_input);
        }

        public static string intToCycleTime(int cycle_time_seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(cycle_time_seconds);
            return string.Format("{0}m {1}s ({2}s)", timeSpan.Minutes, timeSpan.Seconds, cycle_time_seconds);
        }
        #endregion
    }
}
