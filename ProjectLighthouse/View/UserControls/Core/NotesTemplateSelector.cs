using ProjectLighthouse.Model.Core;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public class NotesTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is not FrameworkElement element || item == null)
            {
                return null;
            }

            string key = item.GetType().Name;
            if (item is Note note)
            {
                if (note.SentBy == App.CurrentUser.UserName)
                {
                    key = "rightNote";
                }
                else
                {
                    key = "leftNote";
                }
            }

            return (DataTemplate)element.FindResource(key);
        }
    }
}
