﻿using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class PatientInfo
    {
        public static readonly Color Shading = new(243, 243, 243);

        public void Add(Section section, Patient patient)
        {
            Table table = AddPatientInfoTable(section);

            AddLeftInfo(table.Rows[0].Cells[0], patient);
            AddRightInfo(table.Rows[0].Cells[1], patient);
        }

        private Table AddPatientInfoTable(Section section)
        {
            Table table = section.AddTable();
            table.Shading.Color = Shading;

            table.Rows.LeftIndent = 0;

            table.LeftPadding = Size.TableCellPadding;
            table.TopPadding = Size.TableCellPadding;
            table.RightPadding = Size.TableCellPadding;
            table.BottomPadding = Size.TableCellPadding;

            // Use two columns of equal width
            double columnWidth = Size.GetWidth(section) / 2.0;
            table.AddColumn(columnWidth);
            table.AddColumn(columnWidth);

            // Only one row is needed
            table.AddRow();

            return table;
        }

        private void AddLeftInfo(Cell cell, Patient patient)
        {
            // Add patient name and sex symbol
            Paragraph p1 = cell.AddParagraph();
            p1.Style = CustomStyles.OrderName;
            p1.AddText($"{patient.LastName}, {patient.FirstName}");
            p1.AddSpace(2);
            //AddSexSymbol(p1, patient.Sex);

            // Add patient ID
            Paragraph p2 = cell.AddParagraph();
            p2.AddText("ID: ");
            p2.AddFormattedText(patient.Id, TextFormat.Bold);
        }

        private void AddRightInfo(Cell cell, Patient patient)
        {
            Paragraph p = cell.AddParagraph();

            // Add birthdate
            p.AddText("Birthdate: ");
            p.AddFormattedText(Format(patient.Birthdate), TextFormat.Bold);

            p.AddLineBreak();

            // Add doctor name
            p.AddText("Doctor: ");
            //p.AddFormattedText($"{patient.Doctor.FirstName} {patient.Doctor.LastName}", TextFormat.Bold);
        }

        private string Format(DateTime birthdate)
        {
            return $"{birthdate:d} (age {Age(birthdate)})";
        }

        private int Age(DateTime birthdate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthdate.Year;
            return birthdate.AddYears(age) > today
                ? age - 1
                : age;
        }
    }
}
