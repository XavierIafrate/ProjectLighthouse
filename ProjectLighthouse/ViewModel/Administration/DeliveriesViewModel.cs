using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class DeliveriesViewModel : BaseViewModel
    {
        #region Variables
        private List<DeliveryNote> deliveryNotes;
        public List<DeliveryNote> DeliveryNotes
        {
            get { return deliveryNotes; }
            set
            {
                deliveryNotes = value;
                OnPropertyChanged();
            }
        }

        public List<DeliveryItem> DeliveryItems { get; set; }

        public List<DeliveryItem> FilteredDeliveryItems { get; set; }

        private DeliveryNote selectedDeliveryNote;
        public DeliveryNote SelectedDeliveryNote
        {
            get { return selectedDeliveryNote; }
            set
            {
                selectedDeliveryNote = value;
                if (value == null)
                    return;

                FilteredDeliveryItems = new List<DeliveryItem>(DeliveryItems.Where(n => n.AllocatedDeliveryNote == value.Name));
                OnPropertyChanged(nameof(FilteredDeliveryItems));
                OnPropertyChanged();
            }
        }

        public NewDeliveryCommand CreateDeliveryCommand { get; set; }
        public GeneratePDFDeliveryNoteCommand GenerateDeliveryNotePDFCommand { get; set; }
        #endregion

        public DeliveriesViewModel()
        {
            DeliveryNotes = new();
            DeliveryItems = new();
            SelectedDeliveryNote = new();

            CreateDeliveryCommand = new(this);
            GenerateDeliveryNotePDFCommand = new(this);

            LoadDeliveryItems();
            LoadDeliveryNotes();
        }

        private void LoadDeliveryNotes()
        {
            DeliveryNotes.Clear();
            DeliveryNotes = DatabaseHelper.Read<DeliveryNote>()
                .OrderByDescending(n => n.DeliveryDate)
                .ToList();

            //.Where(d => d.DeliveryDate.AddDays(30) > System.DateTime.Now)

            if (deliveryNotes.Count != 0)
            {
                SelectedDeliveryNote = DeliveryNotes.First();
            }
        }

        private void LoadDeliveryItems()
        {
            DeliveryItems.Clear();
            DeliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();
        }

        public void CreateNewDelivery()
        {
            CreateNewDeliveryWindow window = new();
            window.ShowDialog();
            if (!window.SaveExit)
                return;
            LoadDeliveryNotes();
            LoadDeliveryItems();
            SelectedDeliveryNote = DeliveryNotes.First();
        }

        public void PrintDeliveryNotePDF()
        {
            if (SelectedDeliveryNote != null && FilteredDeliveryItems != null)
            {
                //ReportPdf reportService = new();
                //DeliveryData reportData = new()
                //{
                //    Header = SelectedDeliveryNote,
                //    Lines = FilteredDeliveryItems.ToArray()
                //};
                //string path = GetTempPdfPath();

                //reportService.Export(path, reportData);
                //reportService.OpenPdf(path);
                PDFHelper.PrintDeliveryNote(SelectedDeliveryNote, FilteredDeliveryItems);
            }
        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }
    }
}
