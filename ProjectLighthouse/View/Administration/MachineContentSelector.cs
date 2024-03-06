using ProjectLighthouse.Model.Administration;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public class MachineContentSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is not FrameworkElement element || item == null)
            {
                return null;
            }

            if (item is Lathe)
            {
                return (DataTemplate)element.FindResource("lathe");
            }

            return (DataTemplate)element.FindResource("machine");
        }
    }
}
