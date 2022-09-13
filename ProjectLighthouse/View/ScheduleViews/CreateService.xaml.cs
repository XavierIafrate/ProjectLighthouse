using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.ScheduleViews
{
    public partial class CreateService : Window
    {
        public MachineService Service { get; set; }
        public bool Saved;
        private List<Lathe> Lathes;

        public CreateService(MachineService s, List<Lathe> lathes)
        {
            Service = s;
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


            if (Service.Id > 0)
            {
                PopulateForm();
            }
        }

        private void PopulateForm()
        {
            TitleTextBox.Text = Service.Name;
            TimeSpan allotted = TimeSpan.FromSeconds(Service.TimeToComplete);

            hours.Text = allotted.TotalHours.ToString("0");
            minutes.Text = allotted.Minutes.ToString("0");

            for (int i = 0; i < Lathes.Count; i++)
            {
                Lathe lathe = Lathes[i];
                if (lathe.Id == Service.AllocatedMachine)
                {
                    LathesListBox.SelectedValue = LathesListBox.Items[i];
                    break;
                }
            }

            datePicker.SelectedDate = Service.StartDate;

            TimeComboBox.Text = Service.StartDate.ToString("h:mm tt");
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

            if (Service.Id != 0)
            {
                if (DatabaseHelper.Update(Service))
                {
                    Saved = true;
                }
            }
            else
            {
                if (DatabaseHelper.Insert(Service))
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
                    if (hours_allocated >= 0 && hours_allocated < 1000)
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

            if (string.IsNullOrEmpty(minutes.Text))
            {
                MarkControl(minutes, valid: false);
                valid = false;
            }
            else
            {
                if (int.TryParse(minutes.Text, out int minutes_allocated))
                {
                    if (minutes_allocated >= 0 && minutes_allocated < 60)
                    {
                        MarkControl(minutes, valid: true);
                    }
                    else
                    {
                        MarkControl(minutes, valid: false);
                    }
                }
                else
                {
                    MarkControl(minutes, valid: false);
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
                if (datePicker.SelectedDate > System.DateTime.Now && datePicker.SelectedDate < System.DateTime.Now.AddMonths(13))
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

        private void MarkControl(Control control, bool valid)
        {
            BrushConverter converter = new();
            control.BorderBrush = valid
                ? (Brush)converter.ConvertFromString("#f0f0f0")
                : Brushes.Red;
        }

        private void ImprintData()
        {
            Service.Name = TitleTextBox.Text.Trim();
            Service.TimeToComplete = (int)new TimeSpan(hours: int.Parse(hours.Text), minutes: int.Parse(minutes.Text), seconds: 0).TotalSeconds;

            Lathe selectedLathe = (Lathe)LathesListBox.SelectedValue;
            Service.AllocatedMachine = selectedLathe.Id;

            Service.StartDate = datePicker.SelectedDate.Value;

            ComboBoxItem selectedTime = (ComboBoxItem)TimeComboBox.SelectedItem;
            Service.StartDate = Service.StartDate.AddHours(double.Parse(selectedTime.Tag.ToString()));
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
