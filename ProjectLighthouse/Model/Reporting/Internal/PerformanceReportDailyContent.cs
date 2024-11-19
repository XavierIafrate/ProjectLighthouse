using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class PerformanceReportDailyContent
    {
        public void Add(Section section, DailyPerformance days)
        {
            AddItems(section, days);
        }

        private void AddItems(Section section, DailyPerformance day)
        {
            //section.AddParagraph().AddFormattedText(, StyleNames.Heading2);
            AddTableTitle(section, $"{day.Date:dddd, dd/MM/yyyy}");
            AddItemsTable(section, day);
        }

        private static void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, StyleNames.Heading2);
            p.Format.KeepWithNext = true;
        }

        private void AddItemsTable(Section section, DailyPerformance day)
        {
            Table table = section.AddTable();

            FormatTable(table);
            AddColumnsAndHeaders(table);
            AddItemRows(table, day);

            AddLastRowBorder(table);
            //AlternateRowShading(table);
        }

        private static void FormatTable(Table table)
        {
            table.LeftPadding = 0;
            table.TopPadding = Size.TableCellPadding;
            table.RightPadding = 0;
            table.BottomPadding = Size.TableCellPadding;
            table.Format.LeftIndent = Size.TableCellPadding;
            table.Format.RightIndent = Size.TableCellPadding;
        }

        private void AddColumnsAndHeaders(Table table)
        {
            Unit width = Size.GetWidth(table.Section);
            table.AddColumn(width * 0.1);
            table.AddColumn(width * 0.2);
            table.AddColumn(width * 0.3);
            table.AddColumn(width * 0.4);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Lathe");
            AddHeader(headerRow.Cells[1], "Overall");
            AddHeader(headerRow.Cells[2], "Major Segments");
            AddHeader(headerRow.Cells[3], "Parts Produced Summary");
        }

        private static void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddItemRows(Table table, DailyPerformance days)
        {
            int rowCursor = 1;

            foreach (DailyPerformanceForLathe lathe in days.LathePerformance)
            {
                Tuple<List<Paragraph>, List<Paragraph>> content = GetOperatingPerformanceSegments(lathe.OperatingBlocks);
                List<Paragraph> lotsContent = GetLotsParagraphs(lathe.Lots);

                int numRowsRequired = Math.Max(content.Item1.Count, content.Item2.Count);
                numRowsRequired = Math.Max(numRowsRequired, lotsContent.Count);
                numRowsRequired = Math.Max(numRowsRequired, 1);
                AllocateRows(table, numRowsRequired);

                AddParagraphsToColumn(table, content.Item1, rowCursor, 1);
                AddParagraphsToColumn(table, content.Item2, rowCursor, 2);
                AddParagraphsToColumn(table, lotsContent, rowCursor, 3);

                Paragraph LatheTitle = new();
                LatheTitle.AddText(lathe.Lathe.FullName);
                table.Rows[rowCursor].Cells[0].Add(LatheTitle);

                rowCursor++;
            }
        }

        private static void AllocateRows(Table table, int numRows)
        {
            for (int i = 0; i < numRows; i++)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;
            }

            UnderlineRow(table.Rows[^1]);
        }

        private static void AddParagraphsToColumn(Table table, List<Paragraph> content, int fromRow, int col)
        {
            if (content.Count == 0)
            {
                Row row = table.Rows[fromRow];
                Paragraph nothing = new();
                nothing.AddFormattedText("-no entries-", CustomStyles.GeneratedAtStyle);
                row.Cells[col].Add(nothing);
            }
            else
            {
                for (int i = 0; i < content.Count; i++)
                {
                    Row row = table.Rows[i + fromRow];
                    row.Cells[col].Add(content[i]);
                }
            }
        }

        private static List<Paragraph> GetLotsParagraphs(Lot[] lots)
        {
            List<Paragraph> paragraphs = new();
            Paragraph paragraph;

            for (int i = 0; i < lots.Length; i++)
            {
                paragraph = new();
                Lot lot = lots[i];
                if (lot.IsAccepted || lot.IsDelivered)
                {
                    paragraph.AddFormattedText($"[ACC] {lot.ProductName}, {lot.Quantity:#,##0} pcs", CustomStyles.LotAccepted);
                }
                else if (lot.IsDelivered)
                {
                    paragraph.AddFormattedText($"[DEL] {lot.ProductName}, {lot.Quantity:#,##0} pcs", CustomStyles.LotAccepted);
                }
                else if (lot.IsReject)
                {
                    paragraph.AddFormattedText($"[REJ] {lot.ProductName}, {lot.Quantity:#,##0} pcs", CustomStyles.LotRejected);
                }
                else
                {
                    paragraph.AddFormattedText($"[QUA] {lot.ProductName}, {lot.Quantity:#,##0} pcs", CustomStyles.LotQuarantined);
                }

                paragraphs.Add(paragraph);
            }

            return paragraphs;
        }

        private static Tuple<List<Paragraph>, List<Paragraph>> GetOperatingPerformanceSegments(MachineOperatingBlock[] blocks)
        {
            List<Paragraph> percentages = new();
            List<Paragraph> verbose = new();

            Paragraph tmp;

            Dictionary<string, double> summary = new();

            foreach (MachineOperatingBlock.States state in Enum.GetValues(typeof(MachineOperatingBlock.States)))
            {
                summary.Add(state.ToString(), 0);
            }

            const double secondsInDay = 86399;
            double unknown = secondsInDay; // Time not recorded 

            for (int i = 0; i < blocks.Length; i++)
            {
                MachineOperatingBlock block = blocks[i];
                string state = block.State;
                summary[state] += block.SecondsElapsed;
                unknown -= block.SecondsElapsed;

                if (TimeSpan.FromSeconds(block.SecondsElapsed).TotalHours < 1)
                {
                    continue;
                }

                tmp = new();
                tmp.AddText($"[{block.State}] {block.StateEntered:HH:mm}->{block.StateLeft:HH:mm} ({Math.Round(TimeSpan.FromSeconds(block.SecondsElapsed).TotalHours * 2) / 2:N1}h)");
                verbose.Add(tmp);
            }

            summary[MachineOperatingBlock.States.Unknown.ToString()] = unknown;

            for (int i = 0; i < summary.Count; i++)
            {
                KeyValuePair<string, double> item = summary.ElementAt(i);
                double percentage = item.Value / secondsInDay * 100;

                if (percentage < 0.5)
                {
                    continue;
                }

                tmp = new();
                tmp.AddText($"{item.Key}: {percentage:N1}%");
                percentages.Add(tmp);
            }

            return new(percentages, verbose);
        }

        private static void AddLastRowBorder(Table table)
        {
            Row lastRow = table.Rows[^1];
            lastRow.Borders.Bottom.Width = 2;
        }

        private static void UnderlineRow(Row row)
        {
            row.Borders.Bottom.Width = 1;
        }

        private static void AlternateRowShading(Table table)
        {
            // Start at i = 1 to skip column headers
            for (int i = 1; i < table.Rows.Count; i++)
            {
                if (i % 2 == 0)  // Even rows
                {
                    table.Rows[i].Shading.Color = Color.FromRgb(216, 216, 216);
                }
            }
        }
    }
}
