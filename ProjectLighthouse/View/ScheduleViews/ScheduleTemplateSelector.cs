using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public class ScheduleTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {

            if (container is not FrameworkElement element || item == null)
            {
                return null;
            }

            var key = item.GetType().Name;
            return (DataTemplate)element.FindResource(key);
        }
    }
}
