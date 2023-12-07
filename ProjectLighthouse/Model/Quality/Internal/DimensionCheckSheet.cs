using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Reporting.Internal;
using System;
using System.Collections.Generic;
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
            CheckSheetStyles.Define(document);
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

        public void BuildContent(TechnicalDrawing drawing, List<ToleranceDefinition> dimensions, string? order)
        {
            Section content = new();
            SetupSection(content);

            AddHeader(content);
            AddCheckSheetTable(content, dimensions, drawing, order);
            AddToleranceDescriptions(content, dimensions);

            document.Add(content);

            document.Info.Title = $"Spec_{drawing.DrawingName}R{drawing.Revision}{drawing.AmendmentType}";

            document.Info.Author = "Lighthouse MRP";

            ExportPdf(@"C:\Users\x.iafrate\Documents\checksheet.pdf");
        }

        private Section AddToleranceDescriptions(Section section, List<ToleranceDefinition> dimensions)
        {
            section.AddPageBreak();
            for (int i = 0; i < dimensions.Count; i++)
            {
                Paragraph p = new();
                p.AddFormattedText($"{i.ToString("[0]").PadLeft(6, '\u00A0')} {dimensions[i].Id.PadRight(30, '\u00A0')}{dimensions[i].ToString().PadRight(30, '\u00A0')}{dimensions[i].standard?.Name} ({dimensions[i].standard?.Description})", CheckSheetStyles.ToleranceDef);
                section.Add(p);
            }

            return section;
        }

        private Section AddHeader(Section section)
        {
            Image logo = section.AddImage(new LogoImage().GetMigraDocFileName());
            logo.Width = "6 cm";
            logo.LockAspectRatio = true;
            logo.WrapFormat.Style = WrapStyle.Through;
            logo.RelativeHorizontal = RelativeHorizontal.Margin;


            Paragraph header = section.AddParagraph();
            header.Style = CheckSheetStyles.Title;
            header.Format.Alignment = ParagraphAlignment.Right;

            header.AddText("Inspection Log");
            header.Format.LeftIndent = "1 cm";

            header.Format.SpaceAfter = "5 mm";


            return section;
        }

        private Section AddCheckSheetTable(Section content, List<ToleranceDefinition> dimensions, TechnicalDrawing drawing, string? order)
        {
            Table table = content.AddTable();

            BuildTableFrame(table, content, drawing, order);
            AddRows(table, dimensions);

            table.Borders.DistanceFromTop = "1 cm";

            return new();
        }

        private Table AddRows(Table t, List<ToleranceDefinition> dims)
        {
            Row row;

            for (int i = 0; i < dims.Count; i++)
            {
                if (dims[i] == null)
                {
                    continue;
                }

                row = t.AddRow();

                row.Cells[0].Style = CheckSheetStyles.RowHeader;
                row.Cells[1].Style = CheckSheetStyles.ToleranceDef;
                row.Cells[2].Style = CheckSheetStyles.ToleranceDef;
                row.Cells[3].Style = CheckSheetStyles.ToleranceDef;

                row.Cells[0].AddParagraph(dims[i].Name);
                row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

                row.Cells[1].AddParagraph(dims[i].Nominal);
                row.Cells[1].VerticalAlignment = VerticalAlignment.Center;
                row.Cells[1].Format.Font.Bold = true;

                row.Cells[2].AddParagraph(dims[i].LowerLimit);
                row.Cells[2].VerticalAlignment = VerticalAlignment.Center;

                row.Cells[3].AddParagraph(dims[i].UpperLimit);
                row.Cells[3].Borders.Right.Width = 2;
                row.Cells[3].VerticalAlignment = VerticalAlignment.Center;


                for (int j = 0; j < 14; j++)
                {
                    row.Cells[j].Borders.Width = 1;
                }
            }

            return t;
        }

        private Table BuildTableFrame(Table t, Section c, TechnicalDrawing d, string? o)
        {
            t.Rows.LeftIndent = 0;

            t.LeftPadding = 0;
            t.TopPadding = 0;
            t.RightPadding = 0;
            t.BottomPadding = 0;


            //Console.WriteLine($"Section Width: {c.PageSetup.PageWidth - c.PageSetup.LeftMargin - c.PageSetup.RightMargin}");
            double rowHeadersWidth = 0;
            double thisColWidth = 140;

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


            thisColWidth = (size.Height - c.PageSetup.LeftMargin - c.PageSetup.RightMargin - rowHeadersWidth) / 10;
            Console.WriteLine($"New Col Width {thisColWidth}");

            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);
            t.AddColumn(thisColWidth);


            Row row;

            row = t.AddRow();
            row.HeadingFormat = true;
            if (o is not null)
            {
                row.Cells[0].AddParagraph($"Prepared for order {o}");
                row.Cells[0].Borders.Color = Colors.Black;
                row.Cells[0].Borders.Width = 1;
                row.Cells[0].MergeRight = 1;
                row.Cells[0].MergeDown = 1;
                row.Cells[0].Style = CheckSheetStyles.ToleranceDef;
                row.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            }

            Paragraph p = row.Cells[2].AddParagraph("Page ");
            p.AddPageField();
            p.AddText(" of ");
            p.AddNumPagesField();
            row.Cells[2].Style = CheckSheetStyles.ToleranceDef;
            row.Cells[2].MergeDown = 1;
            row.Cells[2].MergeRight = 1;
            row.Cells[2].VerticalAlignment = VerticalAlignment.Center;
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Borders.Width = 1;



            row.Cells[4].AddParagraph("Sample Inspections");
            row.Cells[4].Borders.Color = Colors.Black;
            row.Cells[4].Borders.Width = 1;
            row.Cells[4].MergeRight = 9;
            row.Cells[4].Style = CheckSheetStyles.ColumnHeader;

            row = t.AddRow();
            row.HeadingFormat = true;

            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].AddParagraph(i.ToString());
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
                row.Cells[i + 3].Style = CheckSheetStyles.ColumnHeader;
            }

            row = t.AddRow();
            row.Cells[2].AddParagraph("Date");
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Format.RightIndent = 5;
            row.Cells[2].Borders.Width = 1;
            row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[2].Style = CheckSheetStyles.ColumnHeader;
            row.Cells[2].MergeRight = 1;

            row.Cells[0].AddParagraph("Specification");
            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Format.RightIndent = 5;
            row.Cells[0].Borders.Width = 1;
            row.Cells[0].Borders.Bottom.Width = 0;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            row.Cells[0].Style = CheckSheetStyles.ColumnHeader;
            row.Cells[0].MergeRight = 1;
            row.Cells[0].MergeDown = 0;

            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }

            row = t.AddRow();
            row.Cells[0].AddParagraph($"Drawing: {d.DrawingName}\nVersion: {d.Revision}{d.AmendmentType}\nApproved Date: {d.ApprovedDate:dd/MM/yyyy}\nApproved By: {d.ApprovedBy}\n\nGenerated {DateTime.Now:s}\nby {App.CurrentUser.GetFullName()}");
            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Format.RightIndent = 5;
            row.Cells[0].Format.LeftIndent = 5;
            row.Cells[0].Borders.Width = 1;
            row.Cells[0].Borders.Top.Width = 0;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            row.Cells[0].Style = CheckSheetStyles.ToleranceDef;
            row.Cells[0].MergeRight = 1;
            row.Cells[0].MergeDown = 6;

            row.Cells[2].AddParagraph("Time");
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Format.RightIndent = 5;
            row.Cells[2].Borders.Width = 1;
            row.Cells[2].Style = CheckSheetStyles.ColumnHeader;
            row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[2].MergeRight = 1;
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }


            row = t.AddRow();
            row.Cells[2].AddParagraph("Checked By");
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Borders.Width = 1;
            row.Cells[2].Format.RightIndent = 5;
            row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[2].Style = CheckSheetStyles.ColumnHeader;
            row.Cells[2].MergeRight = 1;
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }


            row = t.AddRow();
            row.Cells[2].AddParagraph("Calibrated Equipment");
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Format.RightIndent = 5;
            row.Cells[2].Borders.Width = 1;
            row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[2].VerticalAlignment = VerticalAlignment.Center;
            row.Cells[2].Style = CheckSheetStyles.ColumnHeader;
            row.Cells[2].MergeDown = 4;
            row.Cells[2].MergeRight = 1;
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }
            row = t.AddRow();
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }
            row = t.AddRow();
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }
            row = t.AddRow();
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }
            row = t.AddRow();
            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
            }


            row = t.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;

            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Borders.DiagonalDown.Width = 1;
            row.Cells[0].Borders.Width = 1;

            row.Cells[1].AddParagraph("Specification");
            row.Cells[1].Borders.Color = Colors.Black;
            row.Cells[1].Borders.Width = 1;
            row.Cells[1].Borders.Right.Width = 2;
            row.Cells[1].MergeRight = 2;
            row.Cells[1].Style = CheckSheetStyles.ColumnHeader;

            row.Cells[4].AddParagraph("Variation");
            row.Cells[4].Borders.Color = Colors.Black;
            row.Cells[4].Borders.Width = 1;
            row.Cells[4].MergeRight = 9;
            row.Cells[4].Style = CheckSheetStyles.ColumnHeader;


            row = t.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Cells[0].AddParagraph("Feature");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].Format.LeftIndent = 5;
            row.Cells[0].Borders.Color = Colors.Black;
            row.Cells[0].Borders.Width = 1;
            row.Cells[0].Style = CheckSheetStyles.ColumnHeader;


            row.Cells[1].AddParagraph("Nominal");
            row.Cells[1].Borders.Color = Colors.Black;
            row.Cells[1].Borders.Width = 1;
            row.Cells[1].Style = CheckSheetStyles.ColumnHeader;

            row.Cells[2].AddParagraph("Min Limit");
            row.Cells[2].Borders.Color = Colors.Black;
            row.Cells[2].Borders.Width = 1;
            row.Cells[2].Style = CheckSheetStyles.ColumnHeader;

            row.Cells[3].AddParagraph("Max Limit");
            row.Cells[3].Borders.Color = Colors.Black;
            row.Cells[3].Borders.Width = 1;
            row.Cells[3].Borders.Right.Width = 2;
            row.Cells[3].Style = CheckSheetStyles.ColumnHeader;


            for (int i = 1; i < 11; i++)
            {
                row.Cells[i + 3].Borders.Color = Colors.Black;
                row.Cells[i + 3].Borders.Width = 1;
                row.Cells[i + 3].Style = CheckSheetStyles.ColumnHeader;
            }

            //row.BottomPadding = 5;
            row.Borders.Bottom.Width = 2;
            row.Cells[1].Style = CheckSheetStyles.ColumnHeader;


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
