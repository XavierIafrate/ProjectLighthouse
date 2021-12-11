using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

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
                OnPropertyChanged("DeliveryNotes");
            }
        }

        public List<DeliveryItem> deliveryItems { get; set; }


        public List<DeliveryItem> filteredDeliveryItems { get; set; }

        private DeliveryNote selectedDeliveryNote;
        public DeliveryNote SelectedDeliveryNote
        {
            get { return selectedDeliveryNote; }
            set
            {
                selectedDeliveryNote = value;
                if (value == null)
                    return;

                filteredDeliveryItems = new List<DeliveryItem>(deliveryItems.Where(n => n.AllocatedDeliveryNote == value.Name));
                OnPropertyChanged("filteredDeliveryItems");
                OnPropertyChanged("SelectedDeliveryNote");
            }
        }

        public ICommand CreateDeliveryCommand { get; set; }
        public ICommand GenerateDeliveryNotePDFCommand { get; set; }
        #endregion

        public DeliveriesViewModel()
        {
            DeliveryNotes = new List<DeliveryNote>();
            deliveryItems = new List<DeliveryItem>();
            selectedDeliveryNote = new DeliveryNote();

            CreateDeliveryCommand = new NewDeliveryCommand(this);
            GenerateDeliveryNotePDFCommand = new GeneratePDFDeliveryNoteCommand(this);

            LoadDeliveryItems();
            LoadDeliveryNotes();
        }

        private void LoadDeliveryNotes()
        {
            DeliveryNotes.Clear();
            DeliveryNotes = DatabaseHelper.Read<DeliveryNote>().OrderByDescending(n => n.DeliveryDate).ToList();

            if (deliveryNotes.Count != 0)
                SelectedDeliveryNote = DeliveryNotes.First();
        }

        private void LoadDeliveryItems()
        {
            deliveryItems.Clear();
            deliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();
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
            if (SelectedDeliveryNote != null && filteredDeliveryItems != null)
                PDFHelper.PrintDeliveryNote(SelectedDeliveryNote, filteredDeliveryItems);
        }
    }
}
