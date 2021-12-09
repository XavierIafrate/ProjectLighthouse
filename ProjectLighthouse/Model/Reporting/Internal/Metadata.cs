using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
namespace ProjectLighthouse.Model.Reporting.Internal
{
    class Metadata
    {
        public static readonly Color Shading = new(243, 243, 243);

        public void Add(Section section, LatheManufactureOrder order)
        {
            section.AddParagraph($"Manufacture Order: {order.Name}", StyleNames.Heading1);
            section.AddParagraph("Order Details", StyleNames.Heading2);

            Table table = AddMetadataTable(section);

            AddOrderInfo(table, order);
            AddBorders(table);
        }

        private Table AddMetadataTable(Section section)
        {
            Table table = section.AddTable();
            table.Shading.Color = Shading;

            table.Rows.LeftIndent = 0;

            table.LeftPadding = Size.TableCellPadding;
            table.TopPadding = Size.TableCellPadding;
            table.RightPadding = Size.TableCellPadding;
            table.BottomPadding = Size.TableCellPadding;

            double columnWidth = Size.GetWidth(section) / 4.0;
            table.AddColumn(columnWidth);
            table.AddColumn(columnWidth);
            table.AddColumn(columnWidth);
            table.AddColumn(columnWidth);

            return table;
        }

        private void AddOrderInfo(Table table, LatheManufactureOrder order)
        {
            AddRow(table, "Start Date", order.StartDate.ToString("d"), "Tooling Ready", order.IsReady ? "Yes" : "No");
            AddRow(table, "Allocated Machine", order.AllocatedMachine ?? "TBC", "Program Ready", order.HasProgram ? "Yes" : "No");
            AddRow(table, "Created", order.CreatedAt.ToString("d"), "Bar Verified", order.BarIsVerified ? "Yes" : "No");
            AddRow(table, "Estimated # Bars", order.NumberOfBars.ToString("#,##0"), "Bar Allocated", order.BarIsAllocated ? "Yes" : "No");
            AddRow(table, "Bar ID", order.BarID, "Ultrasonic Cleaning", order.ItemNeedsCleaning ? "Yes" : "No");
        }

        private void AddRow(Table table, string parameter1, string value1, string parameter2, string value2)
        {
            table.AddRow();
            table.Rows[^1].Cells[0].AddParagraph(parameter1);
            table.Rows[^1].Cells[1].AddParagraph(value1);
            table.Rows[^1].Cells[2].AddParagraph(parameter2);
            table.Rows[^1].Cells[3].AddParagraph(value2);
        }

        private void AddBorders(Table table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (i % 2 == 0) 
                {
                    table.Rows[i].Shading.Color = Color.FromRgb(216, 216, 216);
                }
            }
        }
    }
}
