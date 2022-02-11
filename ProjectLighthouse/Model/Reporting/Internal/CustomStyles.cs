using MigraDoc.DocumentObjectModel;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class CustomStyles
    {
        public const string GeneratedAtStyle = "GeneratedAtStyle";
        public const string TableTitle = "TableTitle";
        public const string TableSubtitle = "TableSubtitle";
        public const string OrderName = "OrderName";
        public const string ColumnHeader = "ColumnHeader";
        public const string NoRecords = "NoRecords";

        public const string LotQuarantined = "LotQuarantined";
        public const string LotAccepted = "LotAccepted";
        public const string LotRejected = "LotRejected";

        public static void Define(Document doc)
        {
            Style LotQuarantinedStyle = doc.Styles.AddStyle(LotQuarantined, StyleNames.Normal);
            LotQuarantinedStyle.BaseStyle = StyleNames.Normal;
            LotQuarantinedStyle.ParagraphFormat.Font.Size = 8;
            LotQuarantinedStyle.ParagraphFormat.Font.Color = Colors.DarkOrange;

            Style LotAcceptedStyle = doc.Styles.AddStyle(LotAccepted, StyleNames.Normal);
            LotAcceptedStyle.BaseStyle = StyleNames.Normal;
            LotAcceptedStyle.ParagraphFormat.Font.Size = 8;
            LotAcceptedStyle.ParagraphFormat.Font.Color = Colors.DarkGreen;

            Style LotRejectedStyle = doc.Styles.AddStyle(LotRejected, StyleNames.Normal);
            LotRejectedStyle.BaseStyle = StyleNames.Normal;
            LotRejectedStyle.ParagraphFormat.Font.Size = 8;
            LotRejectedStyle.ParagraphFormat.Font.Color = Colors.DarkRed;


            Style genStyle = doc.Styles.AddStyle(GeneratedAtStyle, StyleNames.Normal);
            genStyle.BaseStyle = StyleNames.Normal;
            genStyle.ParagraphFormat.Font.Size = 8;
            genStyle.ParagraphFormat.Font.Color = Colors.Gray;

            Style orderName = doc.Styles.AddStyle(OrderName, StyleNames.Normal);
            orderName.ParagraphFormat.Font.Size = 20;
            orderName.ParagraphFormat.Font.Bold = true;

            Style heading1 = doc.Styles[StyleNames.Heading1];
            heading1.BaseStyle = StyleNames.Normal;
            heading1.Font.Size = 24;
            heading1.ParagraphFormat.SpaceBefore = 20;

            Style heading2 = doc.Styles[StyleNames.Heading2];
            heading2.BaseStyle = StyleNames.Normal;
            heading2.ParagraphFormat.Shading.Color = Color.FromRgb(0, 0, 0);
            heading2.ParagraphFormat.Font.Color = Color.FromRgb(255, 255, 255);
            heading2.ParagraphFormat.Font.Bold = true;
            heading2.ParagraphFormat.SpaceBefore = 10;
            heading2.ParagraphFormat.LeftIndent = Size.TableCellPadding;
            heading2.ParagraphFormat.RightIndent = Size.TableCellPadding;
            heading2.ParagraphFormat.Borders.Distance = Size.TableCellPadding;

            Style columnHeader = doc.Styles.AddStyle(ColumnHeader, StyleNames.Normal);
            columnHeader.ParagraphFormat.Font.Bold = true;
            columnHeader.ParagraphFormat.LeftIndent = Size.TableCellPadding;
            columnHeader.ParagraphFormat.RightIndent = Size.TableCellPadding;

            Style tableTitle = doc.Styles.AddStyle(TableTitle, StyleNames.Normal);
            tableTitle.BaseStyle = StyleNames.Normal;
            //heading2.ParagraphFormat.Shading.Color = Color.FromRgb(0, 0, 0);
            tableTitle.ParagraphFormat.Font.Size = 16;
            tableTitle.ParagraphFormat.Font.Color = Colors.DarkSlateGray;
            tableTitle.ParagraphFormat.Font.Bold = false;
            tableTitle.ParagraphFormat.SpaceBefore = 10;
            tableTitle.ParagraphFormat.LeftIndent = Size.TableCellPadding;
            tableTitle.ParagraphFormat.RightIndent = Size.TableCellPadding;
            tableTitle.ParagraphFormat.Borders.Distance = Size.TableCellPadding;

            Style tableSubtitle = doc.Styles.AddStyle(TableSubtitle, StyleNames.Normal);
            tableSubtitle.BaseStyle = StyleNames.Normal;
            tableSubtitle.ParagraphFormat.Font.Size = 11;
            tableSubtitle.ParagraphFormat.Font.Color = Colors.DarkSlateGray;
            tableSubtitle.ParagraphFormat.Font.Bold = false;
            tableSubtitle.ParagraphFormat.Font.Italic = true;
            tableSubtitle.ParagraphFormat.LeftIndent = Size.TableCellPadding;
            tableSubtitle.ParagraphFormat.RightIndent = Size.TableCellPadding;
            tableSubtitle.ParagraphFormat.SpaceAfter = 5;
            //tableSubtitle.ParagraphFormat.Borders.Distance = Size.TableCellPadding;

            Style noRecords = doc.Styles.AddStyle(NoRecords, StyleNames.Normal);
            noRecords.BaseStyle = StyleNames.Normal;
            noRecords.ParagraphFormat.Font.Size = 12;
            noRecords.ParagraphFormat.Font.Color = Colors.Gray;
            noRecords.ParagraphFormat.Font.Bold = false;
            noRecords.ParagraphFormat.Font.Italic = true;
            noRecords.ParagraphFormat.SpaceBefore = 10;
            noRecords.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            noRecords.ParagraphFormat.LeftIndent = Size.TableCellPadding;
            noRecords.ParagraphFormat.RightIndent = Size.TableCellPadding;
            noRecords.ParagraphFormat.Borders.Distance = Size.TableCellPadding;
        }
    }
}
