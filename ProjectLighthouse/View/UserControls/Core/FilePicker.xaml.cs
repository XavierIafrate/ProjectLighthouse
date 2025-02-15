﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class FilePicker : UserControl, INotifyPropertyChanged
    {
        public string AllowedFileTypes
        {
            get { return (string)GetValue(AllowedFileTypesProperty); }
            set { SetValue(AllowedFileTypesProperty, value); }
        }

        public static readonly DependencyProperty AllowedFileTypesProperty =
            DependencyProperty.Register("AllowedFileTypes", typeof(string), typeof(FilePicker), new PropertyMetadata("PDF Files(*.pdf) | *.pdf"));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(FilePicker), new PropertyMetadata("", SetValues));

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new(prop));
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FilePicker control) return;

            control.fileNameText.Text = string.IsNullOrEmpty(control.FilePath)
                ? "No File Selected"
                : System.IO.Path.GetFileName(control.FilePath);
            control.fileNameText.Foreground = string.IsNullOrEmpty(control.FilePath)
                ? Brushes.Gray
                : (Brush)Application.Current.Resources["OnSurface"];
        }

        public FilePicker()
        {
            InitializeComponent();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = AllowedFileTypes
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() ?? false)
            {
                FilePath = openFileDialog.FileName;
                fileNameText.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
            }
        }
    }
}
