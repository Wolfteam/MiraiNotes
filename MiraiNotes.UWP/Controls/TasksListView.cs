using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Controls
{
    public class TasksListView : ListView
    {
        protected override void PrepareContainerForItemOverride(
            DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (item is TaskModel)
            {
                var binding = new Binding
                {
                    Source = item,
                    Path = new PropertyPath("IsSelected"),
                    Mode = BindingMode.TwoWay
                };

                ((ListViewItem)element).SetBinding(SelectorItem.IsSelectedProperty, binding);
            }
        }
    }
}
