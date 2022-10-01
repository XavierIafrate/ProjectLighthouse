using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using ProjectLighthouse.Model.Orders;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class OrderItemsContent
    {
        public void Add(Section section, LatheManufactureOrderItem[] items)
        {
            AddItems(section, items);
        }

        private void AddItems(Section section, LatheManufactureOrderItem[] items)
        {
            AddTableTitle(section, $"Order Items [{items.Length}]");
            AddItemsTable(section, items);
        }

        private void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, StyleNames.Heading2);
            p.Format.KeepWithNext = true;
        }

        private void AddItemsTable(Section section, LatheManufactureOrderItem[] items)
        {
            Table table = section.AddTable();

            FormatTable(table);
            AddColumnsAndHeaders(table);
            AddItemRows(table, items);

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
            table.AddColumn(width * 0.25);
            table.AddColumn(width * 0.25);
            table.AddColumn(width * 0.25);
            table.AddColumn(width * 0.25);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Product");
            AddHeader(headerRow.Cells[1], "Required");
            AddHeader(headerRow.Cells[2], "Due Date");
            AddHeader(headerRow.Cells[3], "Target");
        }

        private void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddItemRows(Table table, LatheManufactureOrderItem[] items)
        {
            foreach (LatheManufactureOrderItem item in items)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph(item.ProductName);
                row.Cells[1].AddParagraph($"{item.RequiredQuantity:#,##0}");

                if (item.RequiredQuantity > 0)
                {
                    row.Cells[2].AddParagraph($"{item.DateRequired:d}");
                }
                else
                {
                    row.Cells[2].AddParagraph("-");
                }

                row.Cells[3].AddParagraph($"{item.TargetQuantity:#,##0}");
            }
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
