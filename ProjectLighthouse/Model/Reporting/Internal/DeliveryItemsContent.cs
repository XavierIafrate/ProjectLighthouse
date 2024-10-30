using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using ProjectLighthouse.Model.Deliveries;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class DeliveryItemsContent
    {
        const string Address = "Wixroyd Group Ltd.\nAlexia House\nGlenmore Business Park\nChichester\nPO19 7BJ";

        public void Add(Section section, DeliveryItem[] items)
        {
            AddDeliveryInfo(section);
            AddItems(section, items);
            AddSigningLine(section);
        }


        private void AddSigningLine(Section section)
        {
            Table signingLine = section.AddTable();

            Unit width = Size.GetWidth(signingLine.Section);
            signingLine.AddColumn(width * 0.7);

            Row headerRow = signingLine.AddRow();
            signingLine.TopPadding = Size.TableCellPadding * 20;
            signingLine.BottomPadding = Size.TableCellPadding;
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Received By");
        }

        private void AddDeliveryInfo(Section section)
        {
            section.AddParagraph("");
            section.AddParagraph("Deliver To:", CustomStyles.Address);
            section.AddParagraph(Address, CustomStyles.Address);
        }

        private void AddItems(Section section, DeliveryItem[] items)
        {
            AddTableTitle(section, $"Delivery Items [{items.Length}]");
            AddItemsTable(section, items);
        }

        private void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, CustomStyles.TableTitle);
            p.Format.KeepWithNext = true;
        }

        private void AddItemsTable(Section section, DeliveryItem[] items)
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
            table.AddColumn(width * 0.1);
            table.AddColumn(width * 0.3);
            table.AddColumn(width * 0.3);
            table.AddColumn(width * 0.3);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Line #");
            AddHeader(headerRow.Cells[1], "Product");
            AddHeader(headerRow.Cells[2], "Purchase Order Ref.");
            AddHeader(headerRow.Cells[3], "Quantity");
        }

        private void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddItemRows(Table table, DeliveryItem[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph((i + 1).ToString("0"));

                Paragraph p = row.Cells[1].AddParagraph();
                p.AddFormattedText(items[i].Product, CustomStyles.PartNumber);
                p.AddFormattedText(items[i].ExportProductName, CustomStyles.GTIN);

                row.Cells[2].AddParagraph($"{items[i].PurchaseOrderReference}");
                row.Cells[3].AddParagraph($"{items[i].QuantityThisDelivery:#,##0}");
            }
        }

        private void AddLastRowBorder(Table table)
        {
            Row lastRow = table.Rows[^1];
            lastRow.Borders.Bottom.Width = 1;
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
