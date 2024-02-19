using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public class DisplayUnallocatedItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (container is not FrameworkElement element || item == null)
            {
                return null;
            }

            string key = item.GetType().Name;
            return (DataTemplate)element.FindResource(key);
        }
    }
}
