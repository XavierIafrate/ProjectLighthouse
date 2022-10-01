using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using ProjectLighthouse.Model.Core;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class NotesContent
    {
        public void Add(Section section, Note[] notes)
        {
            AddNotes(section, notes);
        }

        private void AddNotes(Section section, Note[] notes)
        {
            AddTableTitle(section, $"Notes [{notes.Length}]");
            AddNotesTable(section, notes);
        }

        private void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, StyleNames.Heading2);
            p.Format.KeepWithNext = true;
        }

        private void AddNotesTable(Section section, Note[] notes)
        {
            Table table = section.AddTable();

            FormatTable(table);
            AddColumnsAndHeaders(table);
            AddNoteRows(table, notes);

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
            table.AddColumn(width * 0.20);
            table.AddColumn(width * 0.15);
            table.AddColumn(width * 0.65);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Time");
            AddHeader(headerRow.Cells[1], "Sent By");
            AddHeader(headerRow.Cells[2], "Note");
        }

        private void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private void AddNoteRows(Table table, Note[] notes)
        {
            foreach (Note note in notes)
            {
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph(System.DateTime.Parse(note.DateSent).ToString("g"));
                if (note.IsEdited)
                {
                    row.Cells[1].AddParagraph(note.SentBy + " [edited]");
                }
                else
                {
                    row.Cells[1].AddParagraph(note.SentBy);
                }
                row.Cells[2].AddParagraph(note.Message);
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
