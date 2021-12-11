using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using ProjectLighthouse.Model.Reporting.Internal;
using System.Diagnostics;
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
            //AddPatientInfo(section, data.Patient);
            AddStructureSet(section, data.StructureSet);
        }

        private void AddPatientInfo(Section section, Patient patient)
        {
            new PatientInfo().Add(section, patient);
        }

        private void AddStructureSet(Section section, StructureSet structureSet)
        {
            new StructureSetContent().Add(section, structureSet);
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
    }
}
