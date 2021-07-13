using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ProjectLighthouse.View.AssemblyViews
{
    /// <summary>
    /// Interaction logic for EditAssemblyOrderWindow.xaml
    /// </summary>
    public partial class EditAssemblyOrderWindow : Window
    {
        private AssemblyManufactureOrder order;
        public AssemblyManufactureOrder Order
        {
            get { return order; }
            set
            {
                order = value;
            }
        }

        private List<Drop> drops;
        public List<Drop> Drops
        {
            get { return drops; }
            set
            {
                drops = value;
            }
        }

        private List<AssemblyOrderItem> items;
        public List<AssemblyOrderItem> Items
        {
            get { return items; }
            set
            {
                items = value;
            }
        }

        private List<Assembly> assemblies;
        public List<Assembly> Assemblies
        {
            get { return assemblies; }
            set { assemblies = value; }
        }

        public List<AssemblyWithCommand> ListBoxAssemblies { get; set; }
        public UpdateAssemblyOrderItemCommand UpdateCommand { get; set; }


        public bool SaveExit { get; set; } = false;

        public EditAssemblyOrderWindow(AssemblyManufactureOrder o, List<Drop> d, List<AssemblyOrderItem> i)
        {
            InitializeComponent();


            Order = o.Clone();
            Drops = new(d);
            Items = new(i);
            Assemblies = AssemblyHelper.CreateAssembliesFromItemList(new(Items));
            ListBoxAssemblies = new();
            UpdateCommand = new(this);

            foreach(Assembly a in Assemblies)
            {

                List<AssemblyItemWithCommands> tmp_children = new();
                foreach (AssemblyOrderItem child in a.Children)
                {
                    tmp_children.Add(new() 
                    { 
                        Child = child,
                        UpdateCommand = UpdateCommand
                    });

                }

                ListBoxAssemblies.Add(new()
                {
                    Parent = a.Parent,
                    ChildDataSet = tmp_children,
                });
            }

            NotesRichTextBox.Document.Blocks.Clear();
            NotesRichTextBox.AppendText(order.Notes);
            NotesRichTextBox.Document.LineHeight = 2;

            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd);
            if (textRange.Text.Length > 2)
                Order.Notes = textRange.Text[0..^2];

            Order.ModifiedAt = DateTime.Now;
            Order.ModifiedBy = App.CurrentUser.GetFullName();

            if (DatabaseHelper.Update(Order))
            {
                SaveExit = true;
                Close();
            }
            
        }

        private void PORef_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Order.POReference = textBox.Text;
            poRefGhostText.Visibility = string.IsNullOrEmpty(textBox.Text) 
                ? Visibility.Visible 
                : Visibility.Hidden;
        }

        private void NotesRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            if (e.Key == Key.F2)
            {
                TextRange range = new(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                range.Text = string.Format("({0:dd/MM/yy HH:mm} - {1}{2}) ",
                    DateTime.Now,
                    App.CurrentUser.FirstName[0].ToString().ToUpper(),
                    App.CurrentUser.LastName[0].ToString().ToUpper());
                Debug.WriteLine(App.CurrentUser.FirstName);
                Debug.WriteLine(App.CurrentUser.FirstName[0].ToString());
                TextPointer caretPos = richTextBox.CaretPosition;
                richTextBox.CaretPosition = caretPos.DocumentEnd;
            }
        }

        public void UpdateItem(AssemblyOrderItem NewValue)
        {
            AssemblyOrderItem OldValue = Items.FirstOrDefault(x => x.ProductName == NewValue.ProductName);
            if (OldValue != null) OldValue = NewValue;
            Order.ModifiedAt = DateTime.Now;
            Order.ModifiedBy = App.CurrentUser.GetFullName();
            DatabaseHelper.Update<AssemblyOrderItem>(NewValue);
            SaveExit = true;
        }
    }
}
