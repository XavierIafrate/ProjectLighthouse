using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class DeliveriesViewModel : BaseViewModel
    {
        #region Variables
        public ObservableCollection<DeliveryNote> deliveryNotes { get; set; }
        public ObservableCollection<DeliveryItem> deliveryItems { get; set; }

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
            deliveryNotes = new ObservableCollection<DeliveryNote>();
            deliveryItems = new ObservableCollection<DeliveryItem>();
            selectedDeliveryNote = new DeliveryNote();

            CreateDeliveryCommand = new NewDeliveryCommand(this);
            GenerateDeliveryNotePDFCommand = new GeneratePDFDeliveryNoteCommand(this);

            LoadDeliveryItems();
            LoadDeliveryNotes();
        }

        private void LoadDeliveryNotes()
        {
            deliveryNotes.Clear();
            List<DeliveryNote> notes = DatabaseHelper.Read<DeliveryNote>().ToList();
            
            foreach(var note in notes)
            {
                deliveryNotes.Add(note);
            }

            deliveryNotes = new ObservableCollection<DeliveryNote>(deliveryNotes.OrderByDescending(n => n.DeliveryDate));
            if(deliveryNotes.Count != 0)
                SelectedDeliveryNote = deliveryNotes.First();
        }

        private void LoadDeliveryItems()
        {
            deliveryItems.Clear();
            List<DeliveryItem> items = DatabaseHelper.Read<DeliveryItem>().ToList();

            foreach (var item in items)
            {
                deliveryItems.Add(item);
            }
        }

        public void CreateNewDelivery()
        {
            CreateNewDeliveryWindow window = new CreateNewDeliveryWindow();
            window.ShowDialog();

            LoadDeliveryItems();
            LoadDeliveryNotes();
        }

        public void PrintDeliveryNotePDF()
        {
            if(SelectedDeliveryNote != null && filteredDeliveryItems != null)
                PDFHelper.PrintDeliveryNote(SelectedDeliveryNote, filteredDeliveryItems);
        }
    }
}
