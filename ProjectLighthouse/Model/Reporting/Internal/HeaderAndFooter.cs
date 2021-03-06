using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using System;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    internal class HeaderAndFooter
    {
        public void Add(Section section)
        {
            AddHeader(section);
            AddFooter(section);
        }

        private void AddHeader(Section section)
        {
            Paragraph header = section.Headers.Primary.AddParagraph();
            header.Format.AddTabStop(Size.GetWidth(section), TabAlignment.Right);
            header.Style = CustomStyles.GeneratedAtStyle;

            Image logo= header.AddImage(new LogoImage().GetMigraDocFileName());
            logo.Width = "8cm";
            logo.LockAspectRatio = true;

            header.AddTab();
            header.AddText("Wixroyd Ltd.");
        }

        private void AddFooter(Section section)
        {
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.Format.AddTabStop(Size.GetWidth(section), TabAlignment.Right);
            footer.Style = CustomStyles.GeneratedAtStyle;

            footer.AddText($"Generated in Lighthouse at {DateTime.Now:g} by {App.CurrentUser.GetFullName()}");
            footer.AddTab();
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();
        }
    }
}
