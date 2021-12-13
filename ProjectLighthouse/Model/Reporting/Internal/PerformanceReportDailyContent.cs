using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
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

        private void AddTableTitle(Section section, string title)
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
            AlternateRowShading(table);
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
            table.AddColumn(width * 0.4);
            table.AddColumn(width * 0.3);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Lathe");
            AddHeader(headerRow.Cells[1], "Overall");
            AddHeader(headerRow.Cells[2], "Major Segments");
            AddHeader(headerRow.Cells[3], "Parts Produced Summary");
        }

        private void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddItemRows(Table table, DailyPerformance days)
        {
            foreach (DailyPerformanceForLathe lathe in days.LathePerformance)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph(lathe.Lathe.Id);
                Tuple<Paragraph, Paragraph> content = GetOperatingPerformanceSegments(lathe.OperatingBlocks);
                row.Cells[1].Add(content.Item1);
                row.Cells[2].Add(content.Item2);
            }
        }

        private Tuple<Paragraph, Paragraph> GetOperatingPerformanceSegments(MachineOperatingBlock[] blocks)
        {
            Paragraph percentages = new();
            Paragraph verbose = new();
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

                verbose.AddText($"[{block.State}] {block.StateEntered:HH:mm}->{block.StateLeft:HH:mm} ({Math.Round(TimeSpan.FromSeconds(block.SecondsElapsed).TotalHours*2) / 2:N1}h)");
                if (i < blocks.Length - 1)
                {
                    verbose.AddLineBreak();
                }
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
                percentages.AddText($"{item.Key}: {percentage:N1}%");
                if (i < summary.Count - 1)
                {
                    percentages.AddLineBreak();
                }
            }

            return new(percentages, verbose);
        }

        private void AddLastRowBorder(Table table)
        {
            Row lastRow = table.Rows[^1];
            lastRow.Borders.Bottom.Width = 2;
        }

        private void AlternateRowShading(Table table)
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
