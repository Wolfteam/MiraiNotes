using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class BaseView
    {
        public const string TaskRecyclerViewId = "TaskRecyclerView";
        public const string MvxListViewClass = "MvxListView";
        public const string MaterialButtonClass = "MaterialButton";

        protected IApp App
            => AppManager.App;
        protected bool OnAndroid
            => AppManager.Platform == Platform.Android;
        protected bool OniOS
            => AppManager.Platform == Platform.iOS;
        protected Color DarkAppThemeColor
            => Color.FromArgb(-14671580);
        protected Color LightAppThemeColor
            => Color.FromArgb(-1);

        public bool IsDrawerOpen()
        {
            return App.Query(x => x.Class("NavigationView")).Any();
        }

        public void PressBackButton()
        {
            App.Back();
        }

        public string GetHint(string fromClass = "TextInputLayout")
        {
            return App.Query(x => x.Class(fromClass).Invoke("getHint"))
                .FirstOrDefault()
                .ToString();
        }

        public Color GetTextColor(AppQuery baseQuery, int index = 0)
        {
            int colorValue = Convert.ToInt32(App.Query(x => baseQuery.Invoke("getCurrentTextColor"))[index]);

            var color = Color.FromArgb(colorValue);
            return color;
        }

        public List<Color> GetTextColors(AppQuery baseQuery)
        {
            var colors = App.Query(x => baseQuery.Invoke("getCurrentTextColor"))
                .Select(value => Color.FromArgb(Convert.ToInt32(value)))
                .ToList();
            return colors;
        }

        public bool ColorsAreClose(Color a, Color b, int threshold = 500)
        {
            var rDist = Math.Abs(a.R - b.R);
            var gDist = Math.Abs(a.G - b.G);
            var bDist = Math.Abs(a.B - b.B);

            if (rDist + gDist + bDist > threshold)
                return false;

            return true;
        }

        public List<Color> GetBackgroundColors(AppQuery baseQuery)
        {
            var colorValues = App.Query(x => baseQuery.Invoke("getBackground").Invoke("getColor"))
                .Select(value => Convert.ToInt32(value))
                .ToList();

            var colors = new List<Color>();
            foreach (var colorValue in colorValues)
            {
                var color = Color.FromArgb(colorValue);
                colors.Add(color);
            }

            return colors;
        }

        public Color GetBackgroundColor(AppQuery baseQuery)
        {
            var noBgColor = Color.FromArgb(0, 0, 0, 0);
            return GetBackgroundColors(baseQuery).First(color => color != noBgColor);
        }

        public AppQuery BuildBaseAppQuery()
        {
            return new AppQuery(QueryPlatform.Android);
        }

        public Color GetAppBarBgColor()
        {
            var query = BuildBaseAppQuery().Class("AppBarLayout");
            return GetBackgroundColor(query);
        }
    }
}
