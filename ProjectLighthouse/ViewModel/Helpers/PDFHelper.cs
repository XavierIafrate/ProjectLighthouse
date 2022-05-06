using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static string DEL_PDF_OUTPUTDIR = @"H:\Production\Administration\Manufacture Records\Delivery Notes";
        private static string SCH_PDF_OUTPUTDIR = @"H:\Production\Administration\Manufacture Records\Schedule Printouts";
        private static string REQ_PDF_OUTPUTDIR = @"H:\Production\Administration\Manufacture Records\Requisition Printouts";
        private static string PFM_PDF_OUTPUTDIR = @"H:\Production\Administration\Manufacture Records\Performance Reports";
        private static string Address = "Automotion Components\nAlexia House\nGlenmore Business Park\nChichester, UK\nP019 7BJ";

        #region Schedule Formats

        private static double DOCUMENT_GUTTER = 30;
        private static double HEADER_HEIGHT = 40;

        private static XFont TITLE_FONT = new("Tahoma", 25, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));
        private static XFont HEADER_FONT = new("Tahoma", 18, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));
        private static XFont DEFAULT_FONT = new("Tahoma", 12, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));

        private static double SCHEDULE_ROW_HEIGHT = 20;
        private static double BAR_ROW_HEIGHT = 20;
        private static double PERFORMANCE_ROW_HEIGHT = 30;

        #endregion Schedule Formats

        #region Schedule

        public static void PrintSchedule(List<LatheManufactureOrder> orders, List<LatheManufactureOrderItem> items, List<Lathe> lathes)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using PdfDocument document = new();

            foreach (Lathe lathe in lathes)
            {
                AppendScheduleForLathe(
                    orders.Where(o => o.AllocatedMachine == lathe.Id && o.State < OrderState.Complete).ToList(),
                    items, lathe, document);
            }

            string fileName = $"schedule_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string path = Directory.Exists(SCH_PDF_OUTPUTDIR)
                ? Path.Join(SCH_PDF_OUTPUTDIR, fileName)
                : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            SavePDF(document, path, true);
        }

        public static PdfDocument AddSchedulePageFrame(PdfDocument doc, Dictionary<string, XRect> columns, Lathe lathe)
        {
            PdfPage page = doc.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Logo
            XImage logo = XImage.FromFile(GetLogoFile());
            double logo_width = 100;
            double logo_aspect_ratio = Convert.ToDouble(logo.PixelHeight) / Convert.ToDouble(logo.PixelWidth);
            double logo_height = logo_width * logo_aspect_ratio;

            double logo_y_position = (HEADER_HEIGHT - logo_height) / 2 + DOCUMENT_GUTTER;

            XRect logoRect = new(DOCUMENT_GUTTER, logo_y_position, logo_width, logo_height);
            gfx.DrawImage(logo, logoRect);

            gfx.DrawString($"SCHEDULE - {lathe.FullName}",
                TITLE_FONT,
                XBrushes.Black,
                new XRect(DOCUMENT_GUTTER + logo_width, DOCUMENT_GUTTER, page.Width - DOCUMENT_GUTTER * 2 - logo_width, HEADER_HEIGHT),
                XStringFormats.CenterRight);

            double cursor_y = DOCUMENT_GUTTER + HEADER_HEIGHT + SCHEDULE_ROW_HEIGHT;

            columns = SetCellHeights(columns, cursor_y);

            // Draw headers

            //gfx.DrawRectangle(XPens.Black, XBrushes.Red, columns["METADATA"]);
            //gfx.DrawRectangle(XPens.Black, XBrushes.Blue, columns["CHECKBOX"]);
            //gfx.DrawRectangle(XPens.Black, XBrushes.Red, columns["PRODUCT"]);
            //gfx.DrawRectangle(XPens.Black, XBrushes.Blue, columns["TARGET"]);
            //gfx.DrawRectangle(XPens.Black, XBrushes.Red, columns["REQUIREMENT"]);

            gfx.DrawString("ORDER",
                HEADER_FONT,
                XBrushes.Black,
                columns["METADATA"],
                XStringFormats.CenterLeft);

            gfx.DrawString("  PRODUCT",
                HEADER_FONT,
                XBrushes.Black,
                columns["PRODUCT"],
                XStringFormats.CenterLeft);

            gfx.DrawString("R",
                HEADER_FONT,
                XBrushes.Black,
                columns["CHECKBOX_RUNNING"],
                XStringFormats.Center);

            gfx.DrawString("C",
                HEADER_FONT,
                XBrushes.Black,
                columns["CHECKBOX_COMPLETE"],
                XStringFormats.Center);

            gfx.DrawString("TARGET",
                HEADER_FONT,
                XBrushes.Black,
                columns["TARGET"],
                XStringFormats.CenterRight);

            gfx.DrawString("REQUIREMENT",
                HEADER_FONT,
                XBrushes.Black,
                columns["REQUIREMENT"],
                XStringFormats.CenterRight);

            cursor_y += SCHEDULE_ROW_HEIGHT;

            gfx.DrawLine(new XPen(XColors.Black, 2),
                new XPoint(DOCUMENT_GUTTER, cursor_y + 0.5 * SCHEDULE_ROW_HEIGHT),
                new XPoint(page.Width - DOCUMENT_GUTTER, cursor_y + 0.5 * SCHEDULE_ROW_HEIGHT));

            gfx.DrawString($"Generated in Lighthouse by {(object)App.CurrentUser.GetFullName()} at {(object)DateTime.Now:dd/MM/yy HH:mm}",
                new XFont("Courier New", 8, XFontStyle.Regular, new(PdfFontEncoding.Unicode)),
                XBrushes.Gray,
                new XRect(DOCUMENT_GUTTER, page.Height - DOCUMENT_GUTTER, page.Width - 2 * DOCUMENT_GUTTER, 20),
                XStringFormats.CenterLeft);

            gfx.Dispose();
            return doc;
        }

        public static PdfDocument AppendScheduleForLathe(List<LatheManufactureOrder> orders, List<LatheManufactureOrderItem> items, Lathe lathe, PdfDocument doc)
        {
            items = items.OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName).ToList();
            orders = orders.OrderBy(n => n.StartDate).Take(Math.Min(4, orders.Count)).ToList();

            Dictionary<string, XRect> columns = new();

            columns.Add("METADATA",
                new(DOCUMENT_GUTTER, 0, 130, SCHEDULE_ROW_HEIGHT));

            columns.Add("CHECKBOX_RUNNING",
                new(DOCUMENT_GUTTER + 130, 0, SCHEDULE_ROW_HEIGHT, SCHEDULE_ROW_HEIGHT));

            columns.Add("CHECKBOX_COMPLETE",
                new(DOCUMENT_GUTTER + 150, 0, SCHEDULE_ROW_HEIGHT, SCHEDULE_ROW_HEIGHT));

            columns.Add("PRODUCT",
                new(DOCUMENT_GUTTER + 170, 0, 140, SCHEDULE_ROW_HEIGHT));

            columns.Add("TARGET",
                new(DOCUMENT_GUTTER + 310, 0, 80, SCHEDULE_ROW_HEIGHT));

            columns.Add("REQUIREMENT",
                new(DOCUMENT_GUTTER + 390, 0, 145, SCHEDULE_ROW_HEIGHT));

            doc = AddSchedulePageFrame(doc, columns, lathe);

            doc = DrawOrdersToSchedule(doc, columns, orders, items, lathe);

            return doc;
        }

        public static PdfDocument DrawOrdersToSchedule(PdfDocument document, Dictionary<string, XRect> cols, List<LatheManufactureOrder> orders, List<LatheManufactureOrderItem> items, Lathe lathe)
        {
            double cursor_y = DOCUMENT_GUTTER + HEADER_HEIGHT + 3 * SCHEDULE_ROW_HEIGHT;

            double pageHeight = document.Pages[0].Height;

            foreach (LatheManufactureOrder order in orders)
            {
                List<LatheManufactureOrderItem> order_items = items.Where(i => i.AssignedMO == order.Name).ToList();

                if (cursor_y + Convert.ToDouble(Math.Max(order_items.Count, 5)) > (pageHeight - DOCUMENT_GUTTER))
                {
                    document = AddSchedulePageFrame(document, cols, lathe);
                    cursor_y = DOCUMENT_GUTTER + HEADER_HEIGHT + 3 * SCHEDULE_ROW_HEIGHT;
                }

                Tuple<PdfDocument, double> res = DrawOrder(order, order_items, cursor_y, document, cols);
                document = res.Item1;
                cursor_y = res.Item2;
            }
            return document;
        }

        public static Tuple<PdfDocument, double> DrawOrder(LatheManufactureOrder order, List<LatheManufactureOrderItem> items, double cursor, PdfDocument document, Dictionary<string, XRect> columns)
        {
            PdfPage page = document.Pages[^1];
            XGraphics gfx = XGraphics.FromPdfPage(page);

            columns = SetCellHeights(columns, cursor);

            int max_rows = Math.Max(4, items.Count);

            for (int i = 0; i < max_rows; i++)
            {
                if (i == 0)
                {
                    gfx.DrawString(order.Name,
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["METADATA"],
                        XStringFormats.CenterLeft);
                }
                else if (i == 1)
                {
                    gfx.DrawString(string.IsNullOrEmpty(order.POReference) ? "TBC" : order.POReference,
                        DEFAULT_FONT,
                        XBrushes.Blue,
                        columns["METADATA"],
                        XStringFormats.CenterLeft);
                }
                else if (i == 2)
                {
                    gfx.DrawString(string.Format("Setting {0:dd/MM/yy}", order.StartDate),
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["METADATA"],
                        XStringFormats.CenterLeft);
                }

                if (i < items.Count)
                {
                    gfx.DrawString("  " + items[i].ProductName,
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["PRODUCT"],
                        XStringFormats.CenterLeft);

                    gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["CHECKBOX_RUNNING"]);
                    gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["CHECKBOX_COMPLETE"]);

                    gfx.DrawString($"{items[i].TargetQuantity:#,##0}",
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["TARGET"],
                        XStringFormats.CenterRight);

                    if (items[i].RequiredQuantity == 0)
                    {
                        gfx.DrawString("--NONE--",
                            DEFAULT_FONT,
                            XBrushes.Gray,
                            columns["REQUIREMENT"],
                            XStringFormats.CenterRight);
                    }
                    else
                    {
                        gfx.DrawString($"{items[i].RequiredQuantity:#,##0} -> {items[i].DateRequired:dd/MM}",
                            DEFAULT_FONT,
                            XBrushes.Black,
                            columns["REQUIREMENT"],
                            XStringFormats.CenterRight);
                    }
                }

                cursor += SCHEDULE_ROW_HEIGHT;
                columns = SetCellHeights(columns, cursor);
            }

            gfx.DrawLine(new XPen(XColors.Black, 1),
                new XPoint(DOCUMENT_GUTTER, cursor + 0.5 * SCHEDULE_ROW_HEIGHT),
                new XPoint(page.Width - DOCUMENT_GUTTER, cursor + 0.5 * SCHEDULE_ROW_HEIGHT));

            cursor += SCHEDULE_ROW_HEIGHT;

            gfx.Dispose();
            return new Tuple<PdfDocument, double>(document, cursor);
        }

        #endregion Schedule

        #region Delivery Note

        public static void PrintDeliveryNote(DeliveryNote deliveryNote, List<DeliveryItem> inputDeliveryItems)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<DeliveryItem> deliveryItems = DeDuplicatedDeliveryItems(inputDeliveryItems);

            using PdfDocument document = new();

            #region Init parameters

            //Debug
            XSolidBrush brush = new(XColor.FromArgb(120, 255, 0, 0));
            XSolidBrush bluebrush = new(XColor.FromArgb(120, 0, 0, 255));

            // Init
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XTextFormatter formatter = new(gfx);
            XPdfFontOptions options = new(PdfFontEncoding.Unicode);

            #endregion Init parameters

            #region Title

            // Logo
            string logo_file = @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\Lighthouse_dark.png";

            if (Environment.UserName == "xavier")
            {
                logo_file = @"C:\Users\xavie\Desktop\Lighthouse_dark.png";
            }

            XImage logo = XImage.FromFile(logo_file);
            const double dy = 150;
            double width = logo.PixelWidth * 72 / logo.HorizontalResolution;
            double height = logo.PixelHeight * 72 / logo.HorizontalResolution;
            width /= 10;
            height /= 10;
            XRect logoRect = new(page.Width / 2 - (width / 2), (dy - height) / 2, width, height);
            gfx.DrawImage(logo, logoRect);

            // Title
            XFont font = new("Tahoma", 35, XFontStyle.Bold, options);
            XRect titleRect = new(page.Width / 2 - 150, 100, 300, 40);
            gfx.DrawString(deliveryNote.Name, font, XBrushes.Black, titleRect, XStringFormats.Center);

            //subtitle
            font = new("Tahoma", 20, XFontStyle.Bold, options);
            titleRect.Y += titleRect.Height;
            gfx.DrawString("DELIVERY NOTE", font, XBrushes.Black, titleRect, XStringFormats.Center);

            #endregion Title

            #region Metadata

            // Manufacturer
            font = new("Tahoma", 12, XFontStyle.Bold, options);
            gfx.DrawString("MANUFACTURER",
                font, XBrushes.Black,
                new XRect(100, 190, 150, 20),
                XStringFormats.CenterLeft);
            font = new("Tahoma", 12, XFontStyle.Regular, options);
            formatter.DrawString(Address, font, XBrushes.Black, new XRect(100, 210, 150, 90));

            // Shipping Information
            font = new("Tahoma", 12, XFontStyle.Bold, options);
            gfx.DrawString("SHIPPING",
                font, XBrushes.Black,
                new XRect(page.Width - 270, 190, 150, 20),
                XStringFormats.CenterLeft);
            font = new("Tahoma", 12, XFontStyle.Regular, options);
            gfx.DrawString($"Shipped by: {deliveryNote.DeliveredBy}",
                font, XBrushes.Black,
                new XRect(page.Width - 270, 190 + 17, 150, 20),
                XStringFormats.CenterLeft);
            gfx.DrawString($"Shipped at: {deliveryNote.DeliveryDate:dd/MM/yyyy HH:mm}",
                font, XBrushes.Black,
                new XRect(page.Width - 270, 190 + 34, 150, 20),
                XStringFormats.CenterLeft);

            #endregion Metadata

            #region Order Details

            XRect subtitleRect = new(page.Width / 2 - 150, 290, 300, 25);
            font = new XFont("Tahoma", 16, XFontStyle.Bold, options);
            gfx.DrawString("DELIVERY DETAILS", font, XBrushes.Black, subtitleRect, XStringFormats.Center);

            int y = 320;
            font = new XFont("Tahoma", 12, XFontStyle.Bold, options);

            XRect RowNumCol = new(0, y, 40, 20);
            XRect PurchaseRefCol = new(0, y, 130, 20);
            XRect ProductCol = new(0, y, 140, 20);
            XRect ThisDelCol = new(0, y, 140, 20);

            double offset = (page.Width - (RowNumCol.Width + PurchaseRefCol.Width + ProductCol.Width + ThisDelCol.Width)) / 2;

            RowNumCol.X = offset;
            PurchaseRefCol.X = RowNumCol.X + RowNumCol.Width;
            ProductCol.X = PurchaseRefCol.X + PurchaseRefCol.Width;
            ThisDelCol.X = ProductCol.X + ProductCol.Width;

            gfx.DrawString("#", font, XBrushes.Black, RowNumCol, XStringFormats.Center);
            gfx.DrawString("Purchase Order", font, XBrushes.Black, PurchaseRefCol, XStringFormats.CenterLeft);
            gfx.DrawString("Product", font, XBrushes.Black, ProductCol, XStringFormats.CenterLeft);
            gfx.DrawString("Quantity", font, XBrushes.Black, ThisDelCol, XStringFormats.Center);
            //gfx.DrawString("Qty. To Follow", font, XBrushes.Black, ToFollowCol, XStringFormats.Center);

            y += 20;

            XPen stroke = new(XColors.Black, 2);
            gfx.DrawLine(stroke, offset, y, page.Width - offset, y);

            y += 3;
            int i = 1;

            font = new("Tahoma", 12, XFontStyle.Regular, options);

            // Init barcode
            BarCode barcode = new Code3of9Standard("EMPTY", new XSize(PurchaseRefCol.Width, ProductCol.Height * 0.8))
            {
                StartChar = '*',
                EndChar = '*'
            };

            foreach (DeliveryItem deliveryItem in deliveryItems)
            {
                if (i % 2 == 0)
                    gfx.DrawRectangle(XPens.Transparent, new XSolidBrush(XColor.FromArgb(26, 8, 89, 152)), new XRect(offset, y, page.Width - 2 * offset, 40));

                RowNumCol.Y = y;
                PurchaseRefCol.Y = y;
                ProductCol.Y = y;
                ThisDelCol.Y = y;
                //ToFollowCol.Y = y;

                gfx.DrawString($"{i}.", font, XBrushes.Black, RowNumCol, XStringFormats.Center);
                gfx.DrawString(deliveryItem.PurchaseOrderReference, font, XBrushes.Black, PurchaseRefCol, XStringFormats.CenterLeft);
                gfx.DrawString(deliveryItem.Product, font, XBrushes.Black, ProductCol, XStringFormats.CenterLeft);
                gfx.DrawString($"{deliveryItem.QuantityThisDelivery:#,##0} pcs", font, XBrushes.Black, ThisDelCol, XStringFormats.Center);
                //gfx.DrawString(string.Format("{0:#,##0} pcs", deliveryItem.QuantityToFollow), font, XBrushes.Black, ToFollowCol, XStringFormats.Center);

                //PO Ref Barcode
                barcode.Text = deliveryItem.PurchaseOrderReference.ToUpper();
                gfx.DrawBarCode(barcode, XBrushes.Black, new XPoint(PurchaseRefCol.X, y + 20));

                // Product # Barcode
                barcode.Text = deliveryItem.Product.ToUpper();
                gfx.DrawBarCode(barcode, XBrushes.Black, new XPoint(ThisDelCol.X, y + 20));

                i += 1;
                y += 40;
            }

            y += 3;
            font = new XFont("Tahoma", 12, XFontStyle.Bold, options);
            gfx.DrawLine(stroke, offset, y, page.Width - offset, y);
            gfx.DrawString("*** END ***", font, XBrushes.Black, new XRect(offset, y, page.Width - 2 * offset, 15), XStringFormats.BottomCenter);

            #endregion Order Details

            #region Footer

            // Signing area
            font = new("Tahoma", 12, XFontStyle.Regular, options);
            stroke = new(XColors.Black, 1);
            y = (int)page.Height - (int)offset - 20;
            gfx.DrawLine(stroke, offset, y, page.Width / 2, y);
            gfx.DrawString("Received by", font, XBrushes.Black, new XRect(offset, y, 100, 20), XStringFormats.CenterLeft);
            gfx.DrawLine(stroke, page.Width / 2 + offset, y, page.Width - offset, y);
            gfx.DrawString("Date received", font, XBrushes.Black, new XRect(page.Width / 2 + offset, y, 100, 20), XStringFormats.CenterLeft);

            // Print stamp
            font = new XFont("Tahoma", 8, XFontStyle.Regular, options);
            XRect footer = new(offset, page.Height - offset, page.Width - 2 * offset, 20);
            gfx.DrawString($"Generated in Lighthouse by {App.CurrentUser.GetFullName()} at {DateTime.Now:dd/MM/yy HH:mm}", font, XBrushes.Black, footer, XStringFormats.BottomLeft);

            #endregion Footer

            string fileName = $"{deliveryNote.Name}_{DateTime.Now:ddMMyy_HHmm}.pdf";
            string path = Directory.Exists(DEL_PDF_OUTPUTDIR) ? Path.Join(DEL_PDF_OUTPUTDIR, fileName) : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            SavePDF(document, path, true);
        }

        private static List<DeliveryItem> DeDuplicatedDeliveryItems(List<DeliveryItem> inputDeliveryItems)
        {
            List<DeliveryItem> result = new();

            foreach (DeliveryItem i in inputDeliveryItems)
            {
                if (result.Where(n => n.ItemManufactureOrderNumber == i.ItemManufactureOrderNumber && n.Product == i.Product).ToList().Count == 0)
                {
                    result.Add(i);
                }
                else
                {
                    foreach (DeliveryItem deliveryItem in result)
                    {
                        if (deliveryItem.ItemManufactureOrderNumber == i.ItemManufactureOrderNumber && deliveryItem.Product == i.Product)
                            deliveryItem.QuantityThisDelivery += i.QuantityThisDelivery;
                    }
                }
            }

            return result;
        }

        #endregion Delivery Note

        #region Bar Requisition

        public static void PrintBarRequisition(List<BarStockRequirementOverview> bars)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            PdfDocument document = new();

            double cursor = DOCUMENT_GUTTER + HEADER_HEIGHT + BAR_ROW_HEIGHT * 3;

            Dictionary<string, double> ColumnDefinitions = new();

            ColumnDefinitions.Add("CHECKBOX", BAR_ROW_HEIGHT);
            ColumnDefinitions.Add("CHECKBOX_PADDING", 10);
            ColumnDefinitions.Add("BAR", 150);
            ColumnDefinitions.Add("ORDER", 70);
            ColumnDefinitions.Add("QTY_REQ", 50);
            ColumnDefinitions.Add("QTY_IN_KASTO", 70);
            ColumnDefinitions.Add("MASS", 70);

            Dictionary<string, XRect> columns = GetCenteredColumns(ColumnDefinitions, BAR_ROW_HEIGHT, 595);

            GetNewBarRequisitionFrame(document, columns);

            foreach (BarStockRequirementOverview bar in bars)
            {
                List<LatheManufactureOrder> unallocated_orders = bar.Orders.Where(o => !o.BarIsAllocated && DateTime.Now.AddDays(14) >= o.StartDate && !string.IsNullOrEmpty(o.AllocatedMachine)).ToList();
                if (unallocated_orders.Count == 0)
                {
                    continue;
                }

                Tuple<PdfDocument, double> res = DrawBarsToRequisition(bar, columns, cursor, document);
                document = res.Item1;
                cursor = res.Item2;
            }

            string fileName = $"requisition_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string path = Directory.Exists(REQ_PDF_OUTPUTDIR)
                ? Path.Join(REQ_PDF_OUTPUTDIR, fileName)
                : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            SavePDF(document, path, true);
        }

        public static PdfDocument GetNewBarRequisitionFrame(PdfDocument doc, Dictionary<string, XRect> cols)
        {
            PdfPage page = new();

            page.Orientation = PdfSharp.PageOrientation.Portrait;

            doc.AddPage(page);

            page = doc.Pages[^1];

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Logo
            XImage logo = XImage.FromFile(GetLogoFile());
            double logo_width = 100;
            double logo_aspect_ratio = Convert.ToDouble(logo.PixelHeight) / Convert.ToDouble(logo.PixelWidth);
            double logo_height = logo_width * logo_aspect_ratio;

            double logo_y_position = (HEADER_HEIGHT - logo_height) / 2 + DOCUMENT_GUTTER;

            XRect logoRect = new(DOCUMENT_GUTTER, logo_y_position, logo_width, logo_height);
            gfx.DrawImage(logo, logoRect);

            gfx.DrawString($"Bar Stock Requisition",
                TITLE_FONT,
                XBrushes.Black,
                new XRect(DOCUMENT_GUTTER + logo_width, DOCUMENT_GUTTER, page.Width - DOCUMENT_GUTTER * 2 - logo_width, HEADER_HEIGHT),
                XStringFormats.CenterRight);

            cols = SetCellHeights(cols, DOCUMENT_GUTTER + HEADER_HEIGHT + BAR_ROW_HEIGHT);

            gfx.DrawString("Bar ID",
                HEADER_FONT,
                XBrushes.Black,
                cols["BAR"],
                XStringFormats.CenterLeft);

            gfx.DrawString("Order",
                HEADER_FONT,
                XBrushes.Black,
                cols["ORDER"],
                XStringFormats.CenterLeft);

            gfx.DrawString("Req.",
                HEADER_FONT,
                XBrushes.Black,
                cols["QTY_REQ"],
                XStringFormats.CenterRight);

            gfx.DrawString("Stock",
                HEADER_FONT,
                XBrushes.Black,
                cols["QTY_IN_KASTO"],
                XStringFormats.CenterRight);

            gfx.DrawString("Mass",
                HEADER_FONT,
                XBrushes.Black,
                cols["MASS"],
                XStringFormats.CenterRight);

            gfx.DrawLine(new XPen(XColors.Black, 1),
                new XPoint(cols["CHECKBOX"].X,
                    y: DOCUMENT_GUTTER
                    + HEADER_HEIGHT
                    + (BAR_ROW_HEIGHT * 2.5)),
                new XPoint(page.Width - cols["CHECKBOX"].X + cols["CHECKBOX"].Width,
                    y: DOCUMENT_GUTTER
                    + HEADER_HEIGHT
                    + (BAR_ROW_HEIGHT * 2.5)));

            gfx.DrawString($"Generated in Lighthouse by {App.CurrentUser.GetFullName()} at {DateTime.Now:dd/MM/yy HH:mm}",
                new XFont("Courier New", 8, XFontStyle.Regular, new(PdfFontEncoding.Unicode)),
                XBrushes.Gray,
                new XRect(DOCUMENT_GUTTER, page.Height - DOCUMENT_GUTTER, page.Width - 2 * DOCUMENT_GUTTER, 20),
                XStringFormats.CenterLeft);

            gfx.Dispose();
            return doc;
        }

        public static Tuple<PdfDocument, double> DrawBarsToRequisition(BarStockRequirementOverview bar, Dictionary<string, XRect> columns, double cursor, PdfDocument doc)
        {
            columns = SetCellHeights(columns, cursor);
            List<LatheManufactureOrder> unallocated_orders = bar.Orders.Where(o => !o.BarIsAllocated && DateTime.Now.AddDays(14) >= o.StartDate && !string.IsNullOrEmpty(o.AllocatedMachine)).ToList();

            //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["CHECKBOX_RUNNING"]);
            int bar_remaining = (int)bar.BarStock.InStock;

            if (bar.BarStock.InStock < unallocated_orders.First().NumberOfBars)
            {
                return new Tuple<PdfDocument, double>(doc, cursor);
            }

            PdfPage page = doc.Pages[^1];
            XGraphics gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString($"{bar.BarStock.Id}",
                DEFAULT_FONT,
                XBrushes.Black,
                columns["BAR"],
                XStringFormats.CenterLeft);
            //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["BAR"]);

            foreach (LatheManufactureOrder order in unallocated_orders)
            {
                bar_remaining -= (int)order.NumberOfBars;

                if (bar_remaining < 0)
                {
                    Debug.WriteLine($"remaining bar {bar_remaining} at order {order.Name} needs {order.NumberOfBars}");
                    break;
                }

                gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["CHECKBOX"]);

                gfx.DrawString($"{order.Name}",
                DEFAULT_FONT,
                XBrushes.Black,
                columns["ORDER"],
                XStringFormats.CenterLeft);

                //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["ORDER"]);

                gfx.DrawString($"{order.NumberOfBars:#,##0}",
                DEFAULT_FONT,
                XBrushes.Black,
                columns["QTY_REQ"],
                XStringFormats.CenterRight);

                //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["QTY_REQ"]);

                gfx.DrawString($"{bar.BarStock.InStock:#,##0}",
                DEFAULT_FONT,
                XBrushes.Black,
                columns["QTY_IN_KASTO"],
                XStringFormats.CenterRight);

                //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["QTY_IN_KASTO"]);
                double order_bar_mass = bar.BarStock.GetUnitMassOfBar() * order.NumberOfBars;
                if (order_bar_mass < 1000)
                {
                    gfx.DrawString($"{order_bar_mass:#,##0kg}",
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["MASS"],
                        XStringFormats.CenterRight);
                }
                else
                {
                    gfx.DrawString($"{order_bar_mass / 1000:0.0T}",
                        DEFAULT_FONT,
                        XBrushes.Black,
                        columns["MASS"],
                        XStringFormats.CenterRight);
                }

                //gfx.DrawRectangle(XPens.Black, XBrushes.Transparent, columns["MASS"]);

                cursor += BAR_ROW_HEIGHT + 5;
                columns = SetCellHeights(columns, cursor);
            }

            cursor += BAR_ROW_HEIGHT;

            gfx.Dispose();
            return new Tuple<PdfDocument, double>(doc, cursor);
        }

        #endregion Bar Requisition

        #region Performance Report

        public static void PrintPerformanceReport(List<MachineOperatingBlock> Statistics, List<Lathe> Lathes, List<LatheManufactureOrder> Orders)
        {
            Statistics = Statistics.OrderBy(s => s.StateEntered).ToList();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            PdfDocument document = new();

            double cursor = DOCUMENT_GUTTER + HEADER_HEIGHT;

            Dictionary<string, double> ColumnDefinitions = new();

            ColumnDefinitions.Add("DAY", 70);
            ColumnDefinitions.Add("OVERALL", 120);
            ColumnDefinitions.Add("BREAKDOWNS", 120);
            ColumnDefinitions.Add("ORDERS", 150);

            Dictionary<string, XRect> columns = GetCenteredColumns(ColumnDefinitions, BAR_ROW_HEIGHT, 595);


            foreach (Lathe lathe in Lathes)
            {
                document = DrawPerformanceReport(Statistics, lathe, Orders, columns, document);
            }

            string fileName = $"performance_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string path = Directory.Exists(PFM_PDF_OUTPUTDIR)
                ? Path.Join(PFM_PDF_OUTPUTDIR, fileName)
                : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            SavePDF(document, path, true);
        }

        private static PdfDocument DrawPerformanceReport(List<MachineOperatingBlock> Statistics, Lathe Lathe, List<LatheManufactureOrder> Orders, Dictionary<string, XRect> cols, PdfDocument doc)
        {
            string timespan = $"{ Statistics.First().StateEntered.Date:d} - {Statistics.Last().StateEntered.Date:d}";
            doc = GetPerformanceReportTemplate(cols, timespan, doc, Lathe.Id);

            PdfPage page = doc.Pages[^1];



            return doc;
        }

        private static PdfDocument GetPerformanceReportTemplate(Dictionary<string, XRect> cols, string TimeStamps, PdfDocument doc, string LatheID)
        {
            PdfPage page = new();
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            _ = doc.AddPage(page);
            page = doc.Pages[^1];
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Logo
            XImage logo = XImage.FromFile(GetLogoFile());
            double logo_width = 100;
            double logo_aspect_ratio = Convert.ToDouble(logo.PixelHeight) / Convert.ToDouble(logo.PixelWidth);
            double logo_height = logo_width * logo_aspect_ratio;

            double logo_y_position = (HEADER_HEIGHT - logo_height) / 2 + DOCUMENT_GUTTER;

            XRect logoRect = new(DOCUMENT_GUTTER, logo_y_position, logo_width, logo_height);
            gfx.DrawImage(logo, logoRect);

            gfx.DrawString($"Performance Report - {LatheID}",
                TITLE_FONT,
                XBrushes.Black,
                new XRect(DOCUMENT_GUTTER + logo_width, DOCUMENT_GUTTER, page.Width - DOCUMENT_GUTTER * 2 - logo_width, HEADER_HEIGHT),
                XStringFormats.CenterRight);
            gfx.DrawString(TimeStamps,
                HEADER_FONT,
                XBrushes.Black,
                new XRect(DOCUMENT_GUTTER + logo_width, DOCUMENT_GUTTER + HEADER_HEIGHT, page.Width - DOCUMENT_GUTTER * 2 - logo_width, HEADER_HEIGHT),
                XStringFormats.CenterRight);

            double cursor = DOCUMENT_GUTTER + HEADER_HEIGHT + HEADER_HEIGHT + PERFORMANCE_ROW_HEIGHT;

            cols = SetCellHeights(cols, cursor);

            gfx.DrawString("Day",
                HEADER_FONT,
                XBrushes.Black,
                cols["DAY"],
                XStringFormats.CenterLeft);

            gfx.DrawString("Performance",
                HEADER_FONT,
                XBrushes.Black,
                cols["OVERALL"],
                XStringFormats.CenterLeft);

            gfx.DrawString("Breakdowns",
                HEADER_FONT,
                XBrushes.Black,
                cols["BREAKDOWNS"],
                XStringFormats.CenterLeft);

            gfx.DrawString("Orders",
                HEADER_FONT,
                XBrushes.Black,
                cols["ORDERS"],
                XStringFormats.CenterLeft);


            cursor += PERFORMANCE_ROW_HEIGHT;

            gfx.DrawLine(new XPen(XColors.Black, 1),
                new XPoint(cols["DAY"].X,
                    y: cursor),
                new XPoint(cols["ORDERS"].X + cols["ORDERS"].Width,
                    y: cursor));

            gfx.DrawString($"Generated in Lighthouse by {App.CurrentUser.GetFullName()} at {DateTime.Now:dd/MM/yy HH:mm}",
                new XFont("Courier New", 8, XFontStyle.Regular, new(PdfFontEncoding.Unicode)),
                XBrushes.Gray,
                new XRect(DOCUMENT_GUTTER, page.Height - DOCUMENT_GUTTER, page.Width - 2 * DOCUMENT_GUTTER, 20),
                XStringFormats.CenterLeft);

            gfx.Dispose();
            return doc;
        }

        #endregion

        #region Helper functions

        private static Dictionary<string, XRect> GetCenteredColumns(Dictionary<string, double> defs, double rowHeight, double pageWidth)
        {
            Dictionary<string, XRect> columns = new();

            double space_required = 0;

            foreach (KeyValuePair<string, double> col in defs)
            {
                space_required += col.Value;
            }

            double i = pageWidth - space_required - 2 * DOCUMENT_GUTTER;
            i /= 2;
            i += DOCUMENT_GUTTER;

            foreach (KeyValuePair<string, double> col in defs)
            {
                columns.Add(col.Key,
                    new(i, 0, col.Value, rowHeight));
                i += col.Value;
            }

            return columns;
        }

        public static Dictionary<string, XRect> SetCellHeights(Dictionary<string, XRect> cols, double y)
        {
            Dictionary<string, XRect> result = new();
            foreach (KeyValuePair<string, XRect> col in cols)
            {
                XRect tmp = new(col.Value.X, y, col.Value.Width, col.Value.Height);
                result.Add(col.Key, tmp);
            }

            return result;
        }

        public static void SavePDF(PdfDocument document, string path, bool open_after)
        {
            try
            {
                document.Save(path);
                MessageBox.Show($"Saved to {path}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                if (open_after)
                    OpenWithDefaultProgram(path);
            }
            catch (Exception e) // No connection to server with adapters installed
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string GetLogoFile()
        {
            return (Environment.UserName == "xavier")
                ? @"C:\Users\xavie\Desktop\Lighthouse_dark.png"
                : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\Lighthouse_dark.png";
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        #endregion Helper functions
    }
}