﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectLighthouse.Model;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayDrawingGroup : UserControl
    {


        public TechnicalDrawingGroup Group
        {
            get { return (TechnicalDrawingGroup)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Group.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(TechnicalDrawingGroup), typeof(DisplayDrawingGroup), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDrawingGroup control)
            {
                return;
            }

            if(control.Group == null)
            {
                return;
            }

            control.ActionRequiredBadge.Visibility = control.Group.Drawings.Where(x => !x.IsApproved && !x.IsRejected && !x.IsWithdrawn).Count() == 0
                ? Visibility.Collapsed
                : Visibility.Visible;

            control.WithdrawnText.Visibility = control.Group.AllDrawingsWithdrawn
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.ArchetypeBadge.Visibility = control.Group.IsArchetypeGroup
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public DisplayDrawingGroup()
        {
            InitializeComponent();
        }
    }
}
