using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Color = System.Drawing.Color;

namespace ProjectLighthouse.Model.Drawings
{
    public class TechnicalDrawing : ICloneable, IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int Revision { get; set; } = 0;
        public string URL { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public bool IsArchetype { get; set; }
        public string Customer { get; set; }
        public string DrawingName { get; set; }
        public string DrawingStore { get; set; }
        public string RawDrawingStore { get; set; }

        public bool IsWithdrawn { get; set; }
        public string WithdrawnBy { get; set; }
        public DateTime WithdrawnDate { get; set; }

        public bool IsRejected { get; set; }
        public string RejectedBy { get; set; }
        public DateTime RejectedDate { get; set; }
        public string RejectionReason { get; set; }

        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public bool ManufacturingApproved { get; set; }
        public string? ManufacturingApprovalBy { get; set; }
        public DateTime? ManufacturingApprovalDate { get; set; }

        public bool EngineeringApproved { get; set; }
        public string? EngineeringApprovalBy { get; set; }
        public DateTime? EngineeringApprovalDate { get; set; }

        public int? TurnedProductId { get; set; }
        public int? GroupId { get; set; }
        public int? MaterialId { get; set; }

        public Type DrawingType { get; set; }
        public Amendment AmendmentType { get; set; }
        public string IssueDetails { get; set; }
        public string SubmissionType { get; set; }
        public bool WatermarkOnly { get; set; }

        [Ignore]
        public bool IsCurrent { get; set; }

        public enum Amendment { A, B, C, D, E, F, G, H }
        public enum Type { Production, Research }


        [Ignore]
        public List<Note> Notes { get; set; }

        public static List<TechnicalDrawing> FindDrawings(List<TechnicalDrawing> drawings, List<LatheManufactureOrderItem> items, int groupId, int materialId)
        {
            List<TechnicalDrawing> drawingsList = new();
            for (int i = 0; i < items.Count; i++)
            {
                TechnicalDrawing d = FindDrawing(drawings, items[i], groupId, materialId);
                if (d != null)
                {
                    drawingsList.Add(d);
                }
            }

            return drawingsList.Distinct().ToList();
        }

        public static TechnicalDrawing? FindDrawing(List<TechnicalDrawing> drawings, LatheManufactureOrderItem item, int groupId, int materialId)
        {
            drawings = drawings.Where(x => x.IsApproved && !x.IsWithdrawn).ToList();

            if (item.IsSpecialPart)
            {
                List<TechnicalDrawing> matches = drawings
                    .Where(d => d.DrawingName == item.ProductName && !d.IsArchetype)
                    .OrderByDescending(d => d.AmendmentType)
                    .OrderByDescending(d => d.Revision)
                    .ToList();

                if (matches.Count > 0)
                {
                    item.DrawingId = matches.First().Id;
                    return matches.First();
                }

                return null;
            }
            else
            {
                List<TechnicalDrawing> matches = drawings
                    .Where(d => d.TurnedProductId == item.ProductId && !d.IsArchetype)
                    .OrderByDescending(d => d.AmendmentType)
                    .OrderByDescending(d => d.Revision).ToList();

                if (matches.Count == 0)
                {
                    return FindArchetype(
                        groupId: groupId,
                        materialId: materialId,
                        drawings: drawings);
                }

                return matches.First();
            }
        }

        private static TechnicalDrawing? FindArchetype(int groupId, int materialId, List<TechnicalDrawing> drawings)
        {
            List<TechnicalDrawing> matches = drawings
                .Where(d =>
                    d.IsArchetype &&
                    d.GroupId == groupId
                    && (d.MaterialId is null || d.MaterialId == materialId))
                .OrderByDescending(d => d.AmendmentType)
                .OrderByDescending(d => d.Revision)
                .ToList();

            if (matches.Count == 0)
            {
                return null;
            }

            if (!matches.Any(x => x.MaterialId == materialId))
            {
                return matches.First();
            }

            matches = matches
                .Where(d => d.MaterialId == materialId)
                .ToList();

            return matches.First();
        }

        public object Clone()
        {
            // TODO review
            return new TechnicalDrawing()
            {
                Id = Id,
                Revision = Revision,
                URL = URL,
                Created = Created,
                CreatedBy = CreatedBy,
                IsArchetype = IsArchetype,
                Customer = Customer,
                DrawingName = DrawingName,
                DrawingStore = DrawingStore,
                RawDrawingStore = RawDrawingStore,
                IsApproved = IsApproved,
                IsRejected = IsRejected,
                RejectedDate = RejectedDate,
                RejectionReason = RejectionReason,
                ApprovedBy = ApprovedBy,
                ApprovedDate = ApprovedDate,
                TurnedProductId = TurnedProductId,
                GroupId = GroupId,
                MaterialId = MaterialId,
                DrawingType = DrawingType,
                AmendmentType = AmendmentType,
                IssueDetails = IssueDetails,
                IsCurrent = IsCurrent,
            };
        }

        public string GetSafeFileName()
        {
            if (Revision == 0)
            {
                return $"{ValidationHelper.MakeValidFileName(DrawingName)}_rc{Id:0}.pdf";
            }
            return $"{ValidationHelper.MakeValidFileName(DrawingName)}_R{Revision:0}{AmendmentType}.pdf";
        }

        public bool PrepareMarkedPdf()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string tmpFile = GetCopyOfRawDocument();

            using var pdfDocument = PdfReader.Open(tmpFile, PdfDocumentOpenMode.Modify);
            PdfPage page = pdfDocument.Pages[0];

            bool success;
            if (IsRejected)
            {
                success = MarkDrawingAsRejected(page);
            }
            else if (!IsApproved)
            {
                success = MarkDrawingAsNotApproved(page);
            }
            else if (DrawingType == Type.Research)
            {
                success = MarkDrawingAsResearchOnly(page);
            }
            else if (IsWithdrawn)
            {
                success = MarkDrawingAsWithdrawn(page);
            }
            else if (IsApproved && !IsCurrent)
            {
                success = MarkDrawingAsSuperceded(page);
            }
            else
            {
                success = MarkDrawingAsApproved(page);
            }

            if (!success)
            {
                return false;
            }

            if (string.IsNullOrEmpty(DrawingStore))
            {
                DrawingStore = @"lib\gen\" + Path.GetRandomFileName();
            }

            pdfDocument.Save(tmpFile); //save && close

            File.Copy(tmpFile, Path.Combine(App.ROOT_PATH, DrawingStore), overwrite: true);
            File.Delete(tmpFile);

            return true;
        }

        private string GetCopyOfRawDocument()
        {
            // Copy the raw drawing store to a known working file and return path

            string baseDrawing = Path.Combine(App.ROOT_PATH, RawDrawingStore);
            string newName = "lib\\" + Path.GetRandomFileName();
            string newCopyName = $@"{App.ROOT_PATH}{newName}.pdf";
            File.Copy(baseDrawing, newCopyName);
            return newCopyName;
        }

        private bool MarkDrawingAsApproved(PdfPage page)
        {
            ApplyApprovalInformation(page);
            return true;
        }
        private bool MarkDrawingAsSuperceded(PdfPage page)
        {
            ApplyWatermark(page, "DRAWING NO LONGER CURRENT", Color.Purple);
            ApplyApprovalInformation(page);
            return true;
        }
        private bool MarkDrawingAsWithdrawn(PdfPage page)
        {
            ApplyWatermark(page, "WITHDRAWN - DO NOT USE", Color.Red);
            ApplyApprovalInformation(page);
            return true;
        }
        private bool MarkDrawingAsResearchOnly(PdfPage page)
        {
            ApplyWatermark(page, "DEVELOPMENT USE ONLY", Color.Blue);
            ApplyApprovalInformation(page);
            return true;
        }
        private bool MarkDrawingAsNotApproved(PdfPage page)
        {
            ApplyWatermark(page, "MANUFACTURE USE NOT PERMITTED", Color.Red);
            return true;
        }
        private bool MarkDrawingAsRejected(PdfPage page)
        {
            ApplyWatermark(page, "REJECTED - DO NOT USE", Color.Red);
            return true;
        }

        private void ApplyApprovalInformation(PdfPage page)
        {
            if (WatermarkOnly) return;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new("Consolas", 9, XFontStyle.Regular);

            gfx.DrawString($"{Revision}{AmendmentType}", font, XBrushes.Black,
                  new XRect(86, 225, 80, 14),
                  XStringFormats.CenterLeft);

            // Style of: RANDY M if too long
            string approvedBy = ApprovedBy.Length <= 18
                ? ApprovedBy.ToUpperInvariant()
                : ApprovedBy.Split(' ')[0].ToUpperInvariant() + " " + ApprovedBy.Split(' ')[1][..1].ToUpperInvariant();

            // unlucky
            if (approvedBy.Length > 18)
            {
                approvedBy = "SEE LIGHTHOUSE";
            }

            gfx.DrawString(approvedBy, font, XBrushes.Black,
                  new XRect(72, 373, 94, 14),
                  XStringFormats.Center);

            gfx.DrawString($"{ApprovedDate:dd/MM/yyyy HH:mm}", font, XBrushes.Black,
                  new XRect(72, 387, 94, 14),
                  XStringFormats.Center);
            gfx.Dispose();
        }

        private static void ApplyWatermark(PdfPage page, string text, Color colour)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new("Consolas", 40, XFontStyle.Bold);
            XBrush brush = new XSolidBrush(XColor.FromArgb((int)(0.20 * 255), colour.R, colour.G, colour.B));

            gfx.DrawString(text, font, brush,
                new XRect(0, 0, page.Width, page.Height),
                  XStringFormats.Center);

            gfx.Dispose();
        }

        public string GetLocalPath()
        {
            return Path.Join(Path.GetTempPath(), GetSafeFileName());
        }

        public bool CopyToAppData()
        {
            string tmpPath = GetLocalPath();

            if (File.Exists(tmpPath))
            {
                try
                {
                    File.Delete(tmpPath);
                    File.Copy(Path.Join(App.ROOT_PATH, DrawingStore), tmpPath);
                }
                catch (IOException)
                {
                    return false;
                }
            }
            else
            {
                File.Copy(Path.Join(App.ROOT_PATH, DrawingStore), tmpPath);
            }

            return true;
        }

        public void ShellOpen()
        {
            System.Diagnostics.Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + GetLocalPath() + "\"";
            _ = fileopener.Start();
        }

        public bool MoveToDrawingStore(string fromPath)
        {
            try
            {
                string bin = @"lib\";
                RawDrawingStore = bin + Path.GetRandomFileName();
                string toPath = Path.Combine(App.ROOT_PATH, RawDrawingStore);
                File.Copy(fromPath, toPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
