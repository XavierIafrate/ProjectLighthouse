using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Reporting.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Paragraph = MigraDoc.DocumentObjectModel.Paragraph;
using Section = MigraDoc.DocumentObjectModel.Section;
using Table = MigraDoc.DocumentObjectModel.Tables.Table;

namespace ProjectLighthouse.Model.Quality.Internal
{
    public class DimensionCheckSheet
    {

        Document document;
        public DimensionCheckSheet()
        {
            document = new();
            CustomStyles.Define(document);
        }

        private void SetupSection(Section section)
        {
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;

            section.PageSetup.LeftMargin = Size.LeftRightPageMargin;
            section.PageSetup.TopMargin = Size.TopBottomPageMargin;
            section.PageSetup.RightMargin = Size.LeftRightPageMargin;
            section.PageSetup.BottomMargin = Size.TopBottomPageMargin;

            section.PageSetup.HeaderDistance = Size.HeaderFooterMargin;
            section.PageSetup.FooterDistance = Size.HeaderFooterMargin;
        }

        public void BuildContent(TechnicalDrawing drawing, List<CheckSheetDimension> dimensions, TurnedProduct product)
        {
            Section content = new();
            SetupSection(content);

            AddHeader(content, drawing);
            AddCheckSheetTable(content, dimensions);

            document.Add(content);

            ExportPdf(@"C:\Users\xavie\Documents\checksheet.pdf");
        }


        private Section AddHeader(Section content, TechnicalDrawing drawing)
        {
            Paragraph paragraph = new();
            paragraph.AddText("Xav's epic check sheet");
            content.Add(paragraph);

            return content;
        }

        private Section AddCheckSheetTable(Section content, List<CheckSheetDimension> dimensions)
        {
            Table table = new();

            BuildTableFrame(table, content);
            AddRows(table, dimensions);

            content.Add(table);

            return new();
        }

        private Table AddRows(Table t, List<CheckSheetDimension> dims)
        {
            Row row;

            for (int i = 0; i < dims.Count; i++)
            {
                row = t.AddRow();
                row.Cells[0].AddParagraph(dims[i].Name);
                row.Cells[1].AddParagraph(dims[i].IsNumeric ? dims[i].NumericValue.ToString(dims[i].StringFormatter) : dims[i].StringValue);
                row.Cells[2].AddParagraph(dims[i].LowerLimit);
                row.Cells[3].AddParagraph(dims[i].UpperLimit);

                for (int j = 0; j < 12; j++)
                {
                    row.Cells[j].Borders.Width = 1;
                }


            }

            return t;
        }

        private Table BuildTableFrame(Table t, Section c)
        {
            t.Rows.LeftIndent = 0;

            t.LeftPadding = Size.TableCellPadding;
            t.TopPadding = Size.TableCellPadding;
            t.RightPadding = Size.TableCellPadding;
            t.BottomPadding = Size.TableCellPadding;


            Console.WriteLine($"Section Width: {c.PageSetup.PageWidth - c.PageSetup.LeftMargin - c.PageSetup.RightMargin}");
            double rowHeadersWidth = 0;
            double thisColWidth = 100;

            t.AddColumn(thisColWidth);
            rowHeadersWidth += thisColWidth;

            thisColWidth = 60;
            t.AddColumn(thisColWidth);
            rowHeadersWidth += thisColWidth;
            t.AddColumn(thisColWidth);
            rowHeadersWidth += thisColWidth;
            t.AddColumn(thisColWidth);
            rowHeadersWidth += thisColWidth;

            Console.WriteLine($"Header occupies {rowHeadersWidth}");


            PdfSharp.Drawing.XSize size = PdfSharp.PageSizeConverter.ToSize(PdfSharp.PageSize.A4);


            thisColWidth = (size.Height - c.PageSetup.LeftMargin - c.PageSetup.RightMargin - rowHeadersWidth) / 8;
            Console.WriteLine($"New Col Width {thisColWidth}");

            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);


            Row row = t.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;

            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Borders.DiagonalDown.Width = 1;
            row.Cells[0].Borders.Width = 1;

            row.Cells[1].AddParagraph("Specification");
            row.Cells[1].Borders.Color = Colors.Black;
            row.Cells[1].Borders.Width = 1;
            row.Cells[1].MergeRight = 2;

            row.Cells[4].AddParagraph("Sample Inspections");
            row.Cells[4].Borders.Color = Colors.Black;
            row.Cells[4].Borders.Width = 1;
            row.Cells[4].MergeRight = 7;

            row = t.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Cells[0].AddParagraph("Feature Name");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Borders.Width = 1;

            row.Cells[1].AddParagraph("Nominal");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[1].Borders.Color = Colors.Black;
            row.Cells[1].Borders.Width = 1;

            row.Cells[2].AddParagraph("Max Limit");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Borders.Width = 1;

            row.Cells[3].AddParagraph("Min Limit");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[3].Borders.Color = Colors.Black;
            row.Cells[3].Borders.Width = 1;

            for (int i = 1; i < 9; i++)
            {
                row.Cells[i + 3].AddParagraph(i.ToString());
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }

            row.BottomPadding = 5;
            row.Borders.Bottom.Width = 2;


            return t;
        }

        private void ExportPdf(string path)
        {
            PdfDocumentRenderer pdfRenderer = new();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(path);
        }
    }
}
