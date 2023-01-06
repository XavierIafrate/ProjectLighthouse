using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Administration
{
    public partial class NewBarWindow : Window
    {
        public BarStock NewBar { get; set; }
        public List<MaterialInfo> Materials { get; set; }

        public NewBarWindow()
        {
            InitializeComponent();
            Materials = DatabaseHelper.Read<MaterialInfo>()
                .OrderBy(x => x.MaterialText)
                .ThenBy(x => x.GradeText)
                .ToList();

            int defaultMaterialId = Materials.Count > 0 ? Materials[0].Id : 0;
            NewBar = new() { MaterialId = defaultMaterialId };
        }

        private void ValidateInt(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void ValidateDouble(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textbox.Text, e);
        }

        private void AddBarButton_Click(object sender, RoutedEventArgs e)
        {
            NewBar.ValidateAll();
            if(NewBar.HasErrors)
            {
                return;
            }

            MessageBox.Show("Adding Bar");
        }
    }
}
