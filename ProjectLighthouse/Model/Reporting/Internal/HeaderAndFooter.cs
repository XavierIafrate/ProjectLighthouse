using MigraDoc.DocumentObjectModel;
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

            header.AddImage(new LogoImage().GetMigraDocFileName());

            header.AddTab();
            header.AddText("Wixroyd International Ltd.");
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
