using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.Deliveries;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
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

        private List<DeliveryNote> filteredDeliveryNotes;

        public List<DeliveryNote> FilteredDeliveryNotes
        {
            get { return filteredDeliveryNotes; }
            set { filteredDeliveryNotes = value; OnPropertyChanged(); }
        }


        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                OnPropertyChanged();
                FilterDeliveries();
            }
        }


        public List<DeliveryItem> DeliveryItems { get; set; }

        public List<DeliveryItem> FilteredDeliveryItems { get; set; }

        private DeliveryNote? selectedDeliveryNote;
        public DeliveryNote? SelectedDeliveryNote
        {
            get { return selectedDeliveryNote; }
            set
            {
                selectedDeliveryNote = value;
                OnPropertyChanged();

                LoadDeliveryNote(value);
            }
        }



        private DeliveryItem selectedDeliveryItem;

        public DeliveryItem SelectedDeliveryItem
        {
            get { return selectedDeliveryItem; }
            set { selectedDeliveryItem = value; OnPropertyChanged(); }
        }


        private bool showingDelivery;
        public bool ShowingDelivery
        {
            get { return showingDelivery; }
            set
            {
                showingDelivery = value;
                OnPropertyChanged();
            }
        }


        public NewDeliveryCommand CreateDeliveryCommand { get; set; }
        public PrintDeliveryNoteCommand GenerateDeliveryNotePDFCommand { get; set; }

        public VerifyDeliveryNoteCommand VerifyCommand { get; set; }
        public EditDeliveryItemCommand EditDeliveryItemCmd { get; set; }


        private bool noteIsNotVerified;
        public bool NoteIsNotVerified
        {
            get { return noteIsNotVerified; }
            set
            {
                noteIsNotVerified = value;
                OnPropertyChanged();
            }
        }

        private bool pdfEnabled;
        public bool PdfEnabled
        {
            get { return pdfEnabled; }
            set
            {
                pdfEnabled = value;
                OnPropertyChanged();
            }
        }



        private Visibility checkingOperaVis;
        public Visibility CheckingOperaVis
        {
            get { return checkingOperaVis; }
            set
            {
                checkingOperaVis = value;
                OnPropertyChanged();
            }
        }

        private bool disableControls;
        public bool DisableControls
        {
            get { return disableControls; }
            set
            {
                disableControls = !value;
                OnPropertyChanged();
            }
        }


        #endregion

        public DeliveriesViewModel()
        {
            InitialiseVariables();

            LoadDeliveryItems();
            LoadDeliveryNotes();

            FilterDeliveries();
        }

        private void InitialiseVariables()
        {
            FilteredDeliveryNotes = new();
            DeliveryNotes = new();
            DeliveryItems = new();
            SelectedDeliveryNote = new();

            CreateDeliveryCommand = new(this);
            GenerateDeliveryNotePDFCommand = new(this);
            EditDeliveryItemCmd = new(this);
            VerifyCommand = new(this);
            DisableControls = false;

            CheckingOperaVis = Visibility.Collapsed;
        }

        private void FilterDeliveries()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredDeliveryNotes = DeliveryNotes.Where(x => x.DeliveryDate.AddMonths(1) > System.DateTime.Now).ToList();
            }
            else
            {
                string userQuery = SearchText.ToUpperInvariant().Replace(" ", "");
                List<string> matchedDeliveryIds = new();

                matchedDeliveryIds.AddRange(DeliveryNotes.Where(x => x.Name.Contains(userQuery)).Select(x => x.Name));
                matchedDeliveryIds.AddRange(DeliveryItems
                    .Where(x => x.Product.Contains(userQuery, StringComparison.InvariantCultureIgnoreCase) || x.PurchaseOrderReference.Contains(userQuery, StringComparison.InvariantCultureIgnoreCase) || (x.ExportProductName ?? string.Empty).Contains(userQuery, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.AllocatedDeliveryNote));


                FilteredDeliveryNotes = DeliveryNotes.Where(x => matchedDeliveryIds.Contains(x.Name)).ToList();
            }

            if (FilteredDeliveryNotes.Count > 0)
            {
                SelectedDeliveryNote = FilteredDeliveryNotes.First();
            }
            else
            {
                SelectedDeliveryNote = null;
            }
        }

        public async void VerifySelectedDeliveryNote()
        {
            if (App.DemoMode)
            {
                MessageBox.Show("This feature is not available in the demo version", "Cannot proceed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedDeliveryNote is null) return;
            CheckingOperaVis = Visibility.Visible;
            DisableControls = true;
            NoteIsNotVerified = false; //disable button

            List<string> problems = await Task.Run(() => OperaHelper.VerifyDeliveryNote(FilteredDeliveryItems));

            CheckingOperaVis = Visibility.Collapsed;
            DisableControls = false;

            SelectedDeliveryNote.Errors = problems;

            if (problems.Count == 0)
            {
                SelectedDeliveryNote.Verified = true;
                DatabaseHelper.Update(SelectedDeliveryNote);
                PdfEnabled = true;
            }
            else
            {
                NoteIsNotVerified = true;

                //string message = "This delivery will not pass goods in according to information available in Opera.\nPlease amend the Purchase Order to proceed.\n\n";
                //for (int i = 0; i < problems.Count; i++)
                //{
                //    message += i == problems.Count - 1 ? problems[i] : problems[i] + "\n";
                //}
                //MessageBox.Show(message, "Mismatch detected", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void LoadDeliveryNotes()
        {
            DeliveryNotes.Clear();
            DeliveryNotes = DatabaseHelper.Read<DeliveryNote>()
                .OrderByDescending(n => n.DeliveryDate)
                .ToList();
        }

        private void LoadDeliveryItems()
        {
            DeliveryItems.Clear();
            DeliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();
        }

        private void LoadDeliveryNote(DeliveryNote? deliveryNote)
        {
            ShowingDelivery = deliveryNote != null;


            if (selectedDeliveryNote == null)
            {
                return;
            }

            NoteIsNotVerified = !selectedDeliveryNote.Verified;
            PdfEnabled = selectedDeliveryNote.Verified || App.CurrentUser.Role == UserRole.Administrator;
            FilteredDeliveryItems = new List<DeliveryItem>(DeliveryItems.Where(n => n.AllocatedDeliveryNote == selectedDeliveryNote.Name));
            OnPropertyChanged(nameof(FilteredDeliveryItems));
        }

        public void CreateNewDelivery()
        {
            CreateNewDeliveryWindow window = new() { Owner = App.MainViewModel.MainWindow };
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
                ReportPdf reportService = new();
                DeliveryData reportData = new()
                {
                    Header = SelectedDeliveryNote,
                    Lines = FilteredDeliveryItems.ToArray()
                };

                string path = GetTempPdfPath();

                reportService.Export(path, reportData);
                reportService.OpenPdf(path);
            }
        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }

        internal void EditDeliveryItem()
        {
            if (SelectedDeliveryItem is null) return;

            EditDeliveryItemWindow editWindow = new(SelectedDeliveryItem);
            editWindow.ShowDialog();

            if (!editWindow.SaveExit) return;

            LoadDeliveryItems();
            LoadDeliveryNote(SelectedDeliveryNote);

        }
    }
}
