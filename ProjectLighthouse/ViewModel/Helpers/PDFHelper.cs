using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using ProjectLighthouse.Model;
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


            Document document = new Document();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Section section = document.AddSection();

            section.AddParagraph("Hello, world");

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.AddFormattedText("Hello, World!", TextFormat.Underline);

            FormattedText ft = paragraph.AddFormattedText("Small text", TextFormat.Bold);
            ft.Font.Size = 6;

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);

            pdfRenderer.Document = document;

            pdfRenderer.RenderDocument();

            string FileName = "HelloWorld.pdf";

            pdfRenderer.PdfDocument.Save(FileName);



            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //string fileName = "test.pdf";
            //using (PdfDocument document = new PdfDocument())
            //{
            //    PdfPage page = document.AddPage();
            //    XGraphics gfx = XGraphics.FromPdfPage(page);


            //    XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);



            //    XFont font = new XFont("Tahoma", 40, XFontStyle.Bold, options);

            //    gfx.DrawString("MANUFACTURE ORDER", font, XBrushes.Black, new XRect(200, 0, page.Width, 100), XStringFormats.Center);
            //    font = new XFont("Tahoma", 20, XFontStyle.Bold, options);

            //    gfx.DrawString("REFERENCE", font, XBrushes.Black, new XRect(0, 100, page.Width, 140), XStringFormats.Center);
            //    gfx.DrawString(order.Name, font, XBrushes.Black, new XRect(0, 140, page.Width, 180), XStringFormats.Center);
            try
            {
                //document.Save(FileName);
                new Process
                {
                    StartInfo = new ProcessStartInfo(FileName)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            catch
            {
                MessageBox.Show("Error, file is probably already open!");
            }




        }

    }
}
