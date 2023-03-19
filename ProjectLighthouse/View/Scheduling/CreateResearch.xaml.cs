using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class CreateResearch : Window
    {
        public ResearchTime Research { get; set; }
        public bool Saved;
        private List<Lathe> Lathes;

        public CreateResearch(ResearchTime r, List<Lathe> lathes)
        {
            Research = r;
            InitializeComponent();

            if (lathes == null)
            {
                Lathes = DatabaseHelper.Read<Lathe>();
            }
            else
            {
                Lathes = lathes;
            }

            LathesListBox.ItemsSource = Lathes;


            if (Research.Id > 0)
            {
                PopulateForm();
            }
        }

        private void PopulateForm()
        {
            TitleTextBox.Text = Research.Name;
            TimeSpan allotted = TimeSpan.FromSeconds(Research.TimeToComplete);

            days.Text = allotted.TotalDays.ToString("0");
            hours.Text = allotted.Hours.ToString("0");

            for (int i = 0; i < Lathes.Count; i++)
            {
                Lathe lathe = Lathes[i];
                if (lathe.Id == Research.AllocatedMachine)
                {
                    LathesListBox.SelectedValue = LathesListBox.Items[i];
                    break;
                }
            }

            datePicker.SelectedDate = Research.StartDate;
            TimeComboBox.Text = Research.StartDate.ToString("h:mm tt");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataIsValid())
            {
                return;
            }
            else
            {
                ImprintData();
            }

            if (Research.Id != 0)
            {
                if (DatabaseHelper.Update(Research))
                {
                    Saved = true;
                }
            }
            else
            {
                if (DatabaseHelper.Insert(Research))
                {
                    Saved = true;
                }
            }

            Close();
        }

        private bool DataIsValid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(TitleTextBox.Text))
            {
                MarkControl(TitleTextBox, valid: false);
                valid = false;
            }
            else
            {
                MarkControl(TitleTextBox, valid: true);
            }

            if (string.IsNullOrEmpty(hours.Text))
            {
                MarkControl(hours, valid: false);
                valid = false;
            }
            else
            {
                if (int.TryParse(hours.Text, out int hours_allocated))
                {
                    if (hours_allocated >= 0 && hours_allocated < 24)
                    {
                        MarkControl(hours, valid: true);
                    }
                    else
                    {
                        MarkControl(hours, valid: false);
                    }
                }
                else
                {
                    MarkControl(hours, valid: false);
                    valid = false;
                }
            }

            if (string.IsNullOrEmpty(days.Text))
            {
                MarkControl(days, valid: false);
                valid = false;
            }
            else
            {
                if (int.TryParse(days.Text, out int minutes_allocated))
                {
                    if (minutes_allocated >= 0 && minutes_allocated < 30)
                    {
                        MarkControl(days, valid: true);
                    }
                    else
                    {
                        MarkControl(days, valid: false);
                    }
                }
                else
                {
                    MarkControl(days, valid: false);
                    valid = false;
                }
            }

            if (LathesListBox.SelectedValue == null)
            {
                MarkControl(LathesListBox, valid: false);
                valid = false;
            }
            else
            {
                MarkControl(LathesListBox, valid: true);
            }

            if (datePicker.SelectedDate == null)
            {
                MarkControl(datePicker, valid: false);
                valid = false;
            }
            else
            {
                if (datePicker.SelectedDate > DateTime.Today.AddDays(-1) && datePicker.SelectedDate < DateTime.Now.AddMonths(13))
                {
                    MarkControl(datePicker, valid: true);
                }
                else
                {
                    MarkControl(datePicker, valid: false);
                    valid = false;
                }
            }

            if (TimeComboBox.SelectedValue == null)
            {
                MarkControl(TimeComboBox, valid: false);
                valid = false;
            }
            else
            {
                MarkControl(TimeComboBox, valid: true);
            }

            return valid;
        }

        private static void MarkControl(Control control, bool valid)
        {
            BrushConverter converter = new();
            control.BorderBrush = valid
                ? (Brush)converter.ConvertFromString("#f0f0f0")
                : Brushes.Red;
        }

        private void ImprintData()
        {
            Research.Name = TitleTextBox.Text.Trim();
            Research.TimeToComplete = (int)new TimeSpan(days: int.Parse(days.Text), hours: int.Parse(hours.Text), minutes: 0, seconds: 0).TotalSeconds;

            Lathe selectedLathe = (Lathe)LathesListBox.SelectedValue;
            Research.AllocatedMachine = selectedLathe.Id;

            Research.StartDate = datePicker.SelectedDate.Value;

            ComboBoxItem selectedTime = (ComboBoxItem)TimeComboBox.SelectedItem;
            Research.StartDate = Research.StartDate.AddHours(double.Parse(selectedTime.Tag.ToString()));
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
