using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class StructureSetContent
    {
        public void Add(Section section, StructureSet structureSet)
        {
            AddHeading(section, structureSet);
            AddStructures(section, structureSet.Structures);
        }

        private void AddHeading(Section section, StructureSet structureSet)
        {
            section.AddParagraph(structureSet.Id, StyleNames.Heading1);
            section.AddParagraph($"Image {structureSet.Image.Id} " +
                                 $"taken {structureSet.Image.CreationTime:g}");
        }

        private void AddStructures(Section section, DataStructure[] structures)
        {
            AddTableTitle(section, "Structures");
            AddStructureTable(section, structures);
        }

        private void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, StyleNames.Heading2);
            p.Format.KeepWithNext = true;
        }

        private void AddStructureTable(Section section, DataStructure[] structures)
        {
            Table table = section.AddTable();

            FormatTable(table);
            AddColumnsAndHeaders(table);
            AddStructureRows(table, structures);

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
            table.AddColumn(width * 0.4);
            table.AddColumn(width * 0.3);
            table.AddColumn(width * 0.3);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Machine");
            AddHeader(headerRow.Cells[1], "Efficiency");
            AddHeader(headerRow.Cells[2], "Good Parts Made");
        }

        private void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddStructureRows(Table table, DataStructure[] structures)
        {
            foreach (DataStructure structure in structures)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph(structure.Id);
                row.Cells[1].AddParagraph($"{structure.Efficiency:f2}");
                row.Cells[2].AddParagraph($"{structure.GoodPartsMade:f2}");
            }
        }

        private void AddLastRowBorder(Table table)
        {
            Row lastRow = table.Rows[table.Rows.Count - 1];
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
