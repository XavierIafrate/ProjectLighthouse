using MigraDoc.DocumentObjectModel;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class CheckSheetStyles
    {
        public static string Title = "Title";
        public static string Subtitle = "Subtitle";
        public static string ColumnHeader = "ColumnHeader";
        public static string RowHeader = "RowHeader";
        public static string ToleranceDef = "ToleranceDef";



        public static void Define(Document doc)
        {
            Style titleStyle = doc.Styles.AddStyle(Title, StyleNames.Normal);
            titleStyle.BaseStyle = StyleNames.Normal;
            titleStyle.ParagraphFormat.Font.Name = "Segoe UI";
            titleStyle.ParagraphFormat.Font.Size = 22;
            titleStyle.ParagraphFormat.Font.Color = Colors.Black;
            titleStyle.ParagraphFormat.Font.Bold = true;
            titleStyle.ParagraphFormat.Font.Italic = false;
            titleStyle.ParagraphFormat.Alignment = ParagraphAlignment.Left;


            Style subtitleStyle = doc.Styles.AddStyle(Subtitle, StyleNames.Normal);
            subtitleStyle.BaseStyle = StyleNames.Normal;
            subtitleStyle.ParagraphFormat.Font.Name = "Segoe UI";
            subtitleStyle.ParagraphFormat.Font.Size = 16;
            subtitleStyle.ParagraphFormat.Font.Color = Colors.Black;
            subtitleStyle.ParagraphFormat.Font.Bold = true;
            subtitleStyle.ParagraphFormat.Font.Italic = false;
            subtitleStyle.ParagraphFormat.Alignment = ParagraphAlignment.Left;


            Style columnHeaderStyle = doc.Styles.AddStyle(ColumnHeader, StyleNames.Normal);
            columnHeaderStyle.BaseStyle = StyleNames.Normal;
            columnHeaderStyle.ParagraphFormat.Font.Name = "Segoe UI";
            columnHeaderStyle.ParagraphFormat.Font.Size = 9;
            columnHeaderStyle.ParagraphFormat.Font.Color = Colors.Black;
            columnHeaderStyle.ParagraphFormat.Font.Bold = true;
            columnHeaderStyle.ParagraphFormat.Font.Italic = false;
            columnHeaderStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;


            Style rowHeaderStyle = doc.Styles.AddStyle(RowHeader, StyleNames.Normal);
            rowHeaderStyle.BaseStyle = StyleNames.Normal;
            rowHeaderStyle.ParagraphFormat.Font.Name = "Segoe UI";
            rowHeaderStyle.ParagraphFormat.Font.Size = 8;
            rowHeaderStyle.ParagraphFormat.Font.Color = Colors.Black;
            rowHeaderStyle.ParagraphFormat.Font.Bold = true;
            rowHeaderStyle.ParagraphFormat.Font.Italic = false;
            rowHeaderStyle.ParagraphFormat.LeftIndent = 5;
            rowHeaderStyle.ParagraphFormat.Alignment = ParagraphAlignment.Left;


            Style toleranceDefStyle = doc.Styles.AddStyle(ToleranceDef, StyleNames.Normal);
            toleranceDefStyle.BaseStyle = StyleNames.Normal;
            toleranceDefStyle.ParagraphFormat.Font.Name = "Consolas";
            toleranceDefStyle.ParagraphFormat.Font.Size = 8;
            toleranceDefStyle.ParagraphFormat.Font.Color = Colors.Black;
            toleranceDefStyle.ParagraphFormat.Font.Bold = true;
            toleranceDefStyle.ParagraphFormat.Font.Italic = false;
            toleranceDefStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;


        }
    }
}
