﻿using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using ProjectLighthouse.Model.Core;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class LoginsContent
    {
        public void Add(Section section, UserLoginRecords userLogins)
        {
            AddItems(section, userLogins);
        }

        private static void AddItems(Section section, UserLoginRecords user)
        {
            AddTableTitle(section, user.User.GetFullName());
            AddTableSubtitle(section, user.User.Role.ToString());
            if (user.UserLogins.Count > 0)
            {
                AddItemsTable(section, user.UserLogins);
            }
            else
            {
                AddNoItemsIndicator(section);
            }
        }

        private static void AddNoItemsIndicator(Section section)
        {
            Paragraph p = section.AddParagraph("No logins", CustomStyles.NoRecords);
            p.Format.KeepWithNext = true;
        }

        private static void AddTableTitle(Section section, string title)
        {
            Paragraph p = section.AddParagraph(title, CustomStyles.TableTitle);
            p.Format.KeepWithNext = true;
        }

        private static void AddTableSubtitle(Section section, string subtitle)
        {
            Paragraph p = section.AddParagraph(subtitle, CustomStyles.TableSubtitle);
            p.Format.KeepWithNext = true;
        }

        private static void AddItemsTable(Section section, List<Login> logins)
        {
            Table table = section.AddTable();

            FormatTable(table);
            AddColumnsAndHeaders(table);
            AddItemRows(table, logins);

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
            table.KeepTogether = true;
        }

        private static void AddColumnsAndHeaders(Table table)
        {
            Unit width = Size.GetWidth(table.Section);
            table.AddColumn(width * 0.2);
            table.AddColumn(width * 0.15);
            table.AddColumn(width * 0.08);
            table.AddColumn(width * 0.12);
            table.AddColumn(width * 0.15);
            table.AddColumn(width * 0.15);
            table.AddColumn(width * 0.15);

            Row headerRow = table.AddRow();
            headerRow.Borders.Bottom.Width = 1;

            AddHeader(headerRow.Cells[0], "Host");
            AddHeader(headerRow.Cells[1], "AD User");
            AddHeader(headerRow.Cells[2], "Version");
            AddHeader(headerRow.Cells[3], "Date");
            AddHeader(headerRow.Cells[4], "Time In");
            AddHeader(headerRow.Cells[5], "Time Out");
            AddHeader(headerRow.Cells[6], "Session");
        }

        private static void AddHeader(Cell cell, string header)
        {
            Paragraph p = cell.AddParagraph(header);
            p.Style = CustomStyles.ColumnHeader;
        }

        private static void AddItemRows(Table table, List<Login> logins)
        {
            int rowCursor = 1;

            int numRowsRequired = logins.Count;
            AllocateRows(table, numRowsRequired);

            table.Rows[0].KeepWith = Math.Min(numRowsRequired, 5);

            foreach (Login login in logins)
            {
                Row row = table.Rows[rowCursor];
                Paragraph p = new();
                p.AddText(login.Host ?? "n/a");
                row.Cells[0].Add(p);

                p = new();
                p.AddText(login.ADUser ?? "n/a");
                row.Cells[1].Add(p);

                p = new();
                p.AddText(login.Version ?? "n/a");
                row.Cells[2].Add(p);

                p = new();
                p.AddText(login.Time.ToString("ddd dd/MM"));
                row.Cells[3].Add(p);

                p = new();
                p.AddText(login.Time.ToString("HH:mm:ss tt"));
                row.Cells[4].Add(p);

                p = new();
                string logoutText = login.Logout > DateTime.MinValue ? login.Logout.ToString("HH:mm:ss tt") : "n/a";
                p.AddText(logoutText);
                row.Cells[5].Add(p);

                p = new();
                p.AddText(login.GetSessionDuration() ?? "n/a");
                row.Cells[6].Add(p);

                rowCursor++;
            }
        }

        private static void AlternateRowShading(Table table)
        {
            // Start at i = 1 to skip column headers
            for (int i = 1; i < table.Rows.Count; i++)
            {
                if (i % 2 == 0)  // Even rows
                {
                    table.Rows[i].Shading.Color = Color.FromRgb(230, 230, 230);
                }
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

        private static void UnderlineRow(Row row)
        {
            row.Borders.Bottom.Width = 1;
        }

        private static void AddLastRowBorder(Table table)
        {
            Row lastRow = table.Rows[^1];
            lastRow.Borders.Bottom.Width = 2;
        }
    }
}
