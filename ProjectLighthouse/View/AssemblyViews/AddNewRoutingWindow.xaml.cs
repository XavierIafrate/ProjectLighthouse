﻿using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.AssemblyViews
{
    /// <summary>
    /// Interaction logic for AddNewRoutingWindow.xaml
    /// </summary>
    public partial class AddNewRoutingWindow : Window
    {
        private AssemblyItem product;

        public AssemblyItem Product
        {
            get { return product; }
            set 
            { 
                product = value;
                this.Title = string.Format("New Routing: {0}", product.Routing);
            }
        }

        public bool wasSaved = false;
        private Routing newRouting { get; set; }
        public AddNewRoutingWindow()
        {
            InitializeComponent();
            newRouting = new Routing(); // go figure
        }

        private void AssignValues()
        {
            int j = 0;
            newRouting.RoutingID = product.Routing;
            newRouting.Description = DescriptionTextBox.Text;
            newRouting.Workstation = WorkstationComboBox.Text;
            if(Int32.TryParse(SetupTimeTextBox.Text, out j))
                newRouting.SetupTime = j;

            if (Int32.TryParse(CycleTimeTextBox.Text, out j))
                newRouting.CycleTime = j;

            if (Int32.TryParse(QuantityPerCycleTextBox.Text, out j))
                newRouting.QuantityPerCycle = j;

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AssignValues();
            if (ValidateRouting())
            {
                DatabaseHelper.Insert<Routing>(newRouting);
                wasSaved = true;
                this.Close();
            }
        }

        private bool ValidateRouting()
        {
            if (string.IsNullOrWhiteSpace(newRouting.Description))
            {
                MessageBox.Show("Description cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(newRouting.Workstation))
            {
                MessageBox.Show("Workstation cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (newRouting.SetupTime <=0 )
            {
                MessageBox.Show("Setup Time cannot be <= 0", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (newRouting.CycleTime <= 0)
            {
                MessageBox.Show("Cycle Time cannot be <= 0", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (newRouting.QuantityPerCycle <= 0)
            {
                MessageBox.Show("Batch quantity cannot be less than one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}