using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling
{
    public class ScheduleTemplateSelector : DataTemplateSelector
    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
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
