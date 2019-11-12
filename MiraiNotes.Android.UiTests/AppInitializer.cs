using System;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp.Android.StartApp();
            }
            throw new NotImplementedException("Ios is not implemented");
        }
    }
}