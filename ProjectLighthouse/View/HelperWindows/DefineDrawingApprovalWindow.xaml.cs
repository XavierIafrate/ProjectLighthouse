﻿using ABI.Windows.ApplicationModel.Activation;
using ProjectLighthouse.Model;
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

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class DefineDrawingApprovalWindow : Window
    {
        public bool Confirmed;
        public TechnicalDrawing DrawingToApprove;
        public List<TechnicalDrawing> DrawingsInGroup;

        public DefineDrawingApprovalWindow()
        {
            InitializeComponent();
        }

        public void SetupInterface()
        {
            int MaxRev = DrawingsInGroup.Max(x => x.Revision);
            TechnicalDrawing.Amendment maxAmd = DrawingsInGroup.Max(x => x.AmendmentType);

            revPreview.Text = $"Rev.{MaxRev + 1}{TechnicalDrawing.Amendment.A}";
            amdPreview.Text = $"Rev.{MaxRev}{maxAmd+1}";
        }

        private void amendmentOpt_Checked(object sender, RoutedEventArgs e)
        {
            revisionOpt.IsChecked = false;
            amdBox.Background = (Brush)App.Current.Resources["SelectedElement"];
        }

        private void revisionOpt_Checked(object sender, RoutedEventArgs e)
        {
            amendmentOpt.IsChecked = false;
            revBox.Background = (Brush)App.Current.Resources["SelectedElement"];
        }

        private void amendmentOpt_Unchecked(object sender, RoutedEventArgs e)
        {
            amdBox.Background = Brushes.White;
        }

        private void revisionOpt_Unchecked(object sender, RoutedEventArgs e)
        {
            revBox.Background = Brushes.White;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)amendmentOpt.IsChecked && !(bool)revisionOpt.IsChecked)
            {
                MessageBox.Show("Choose one");
                return;
            }

            int MaxRev = DrawingsInGroup.Max(x => x.Revision);
            TechnicalDrawing.Amendment maxAmd = DrawingsInGroup.Max(x => x.AmendmentType);

            if ((bool)amendmentOpt.IsChecked)
            {
                DrawingToApprove.Revision = MaxRev;
                DrawingToApprove.AmendmentType = maxAmd + 1;
            }
            else
            {
                DrawingToApprove.Revision = MaxRev + 1;
                DrawingToApprove.AmendmentType = TechnicalDrawing.Amendment.A;
            }
            Confirmed = true;
            Close();
        }
    }
}