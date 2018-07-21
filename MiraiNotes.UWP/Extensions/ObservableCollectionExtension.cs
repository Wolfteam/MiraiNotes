using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MiraiNotes.UWP.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> col)
        {
            return new ObservableCollection<T>(col);
        }
    }
}
