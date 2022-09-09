using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using ProjectLighthouse.Model.Reporting.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProjectLighthouse.Model.Reporting
{
    public class ReportPdf : IReport
    {
        #region Core Functions
        private void SetUpPage(Section section)
        {
            section.PageSetup.PageFormat = PageFormat.Letter;

            section.PageSetup.LeftMargin = Size.LeftRightPageMargin;
            section.PageSetup.TopMargin = Size.TopBottomPageMargin;
            section.PageSetup.RightMargin = Size.LeftRightPageMargin;
            section.PageSetup.BottomMargin = Size.TopBottomPageMargin;

            section.PageSetup.HeaderDistance = Size.HeaderFooterMargin;
            section.PageSetup.FooterDistance = Size.HeaderFooterMargin;
        }

        private void AddHeaderAndFooter(Section section)
        {
            new HeaderAndFooter().Add(section);
        }

        public void OpenPdf(string path)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            _ = fileopener.Start();
        }
        private void ExportPdf(string path, Document report)
        {
            PdfDocumentRenderer pdfRenderer = new();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            pdfRenderer.Document = report;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(path);
        }

        #endregion

        #region Performance Report

        public void Export(string path, PerformanceReportData data)
        {
            ExportPdf(path, CreateReport(data));
        }

        private Document CreateReport(PerformanceReportData data)
        {
            Document doc = new();
            CustomStyles.Define(doc);
            doc.Add(CreateMainSection(data));
            return doc;
        }

        private Section CreateMainSection(PerformanceReportData data)
        {
            Section section = new();
            SetUpPage(section);
            AddHeaderAndFooter(section);
            AddContents(section, data);
            return section;
        }

        private void AddContents(Section section, PerformanceReportData data)
        {
            AddReportMetadata(section, data);
            for (int i = 0; i < data.Days.Count; i++)
            {
                AddDaysPerformance(section, data.Days[i]);
            }
        }

        private void AddReportMetadata(Section section, PerformanceReportData data)
        {
            new Metadata().Add(section, data);
        }

        private void AddDaysPerformance(Section section, DailyPerformance day)
        {
            new PerformanceReportDailyContent().Add(section, day);
        }

        #endregion

        #region Order Printout
        public void Export(string path, OrderPrintoutData data)
        {
            ExportPdf(path, CreateOrderPrintout(data));
        }

        private Document CreateOrderPrintout(OrderPrintoutData data)
        {
            Document doc = new();
            CustomStyles.Define(doc);
            doc.Add(CreateMainSection(data));
            return doc;
        }

        private Section CreateMainSection(OrderPrintoutData data)
        {
            Section section = new();
            SetUpPage(section);
            AddHeaderAndFooter(section);
            AddContents(section, data);
            return section;
        }

        private void AddContents(Section section, OrderPrintoutData data)
        {
            AddOrderMetadata(section, data.Order);
            AddOrderItems(section, data.Items);
            if (data.Notes.Length > 0)
            {
                AddOrderNotes(section, data.Notes);
            }
        }

        private void AddOrderMetadata(Section section, LatheManufactureOrder order)
        {
            new Metadata().Add(section, order);
        }

        private void AddOrderItems(Section section, LatheManufactureOrderItem[] items)
        {
            new OrderItemsContent().Add(section, items);
        }
        private void AddOrderNotes(Section section, Note[] notes)
        {
            new NotesContent().Add(section, notes);
        }

        #endregion

        #region Delivery Note
        public void Export(string path, DeliveryData data)
        {
            ExportPdf(path, CreateOrderPrintout(data));
        }

        private Document CreateOrderPrintout(DeliveryData data)
        {
            Document doc = new();
            CustomStyles.Define(doc);
            doc.Add(CreateMainSection(data));
            return doc;
        }

        private Section CreateMainSection(DeliveryData data)
        {
            Section section = new();
            SetUpPage(section);
            AddHeaderAndFooter(section);
            AddContents(section, data);
            return section;
        }


        private void AddContents(Section section, DeliveryData data)
        {
            AddOrderMetadata(section, data.Header);
            AddDeliveryItems(section, data.Lines);
        }

        private void AddOrderMetadata(Section section, DeliveryNote delivery)
        {
            new Metadata().Add(section, delivery);
        }

        private void AddDeliveryItems(Section section, DeliveryItem[] items)
        {
            //Aggregate
            string[] unique = items.Select(x => x.Product).Distinct().ToArray();
            DeliveryItem[] uniqueItems = new DeliveryItem[unique.Length];
            for (int i = 0; i < unique.Length; i++)
            {
                uniqueItems[i] = items.Where(x => x.Product == unique[i]).First();
                uniqueItems[i].QuantityThisDelivery = items.Where(x => x.Product == unique[i]).Sum(x => x.QuantityThisDelivery);
            }

            new DeliveryItemsContent().Add(section, uniqueItems);
        }

        #endregion

        #region Logins
        public void Export(string path, LoginReportData data)
        {
            ExportPdf(path, CreateLoginReport(data));
        }

        private Document CreateLoginReport(LoginReportData data)
        {
            Document doc = new();
            CustomStyles.Define(doc);
            doc.Add(CreateMainSection(data));
            return doc;
        }

        private Section CreateMainSection(LoginReportData data)
        {
            Section section = new();
            SetUpPage(section);
            AddHeaderAndFooter(section);
            AddContents(section, data);
            return section;
        }

        private void AddContents(Section section, LoginReportData data)
        {
            AddReportMetadata(section, data);
            AddLogins(section, data.UserLogins);
        }

        private void AddReportMetadata(Section section, LoginReportData data)
        {
            new Metadata().Add(section, data);
        }

        private void AddLogins(Section section, List<UserLoginRecords> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                new LoginsContent().Add(section, items[i]);
            }
        }

        #endregion
    }
}
