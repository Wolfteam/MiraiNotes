using MiraiNotes.Core.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MiraiNotes.UWP.Controls.DataTemplates
{
    public class AccentColorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AccentColorTemplate
        {
            get;
            set;
        }

        public DataTemplate SystemAccentColorTemplate
        {
            get;
            set;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (!(item is AccentColorModel accentColor))
            {
                return this.AccentColorTemplate;
            }

            return accentColor.IsSystemAccentColor ? SystemAccentColorTemplate : AccentColorTemplate;
        }
    }
}
