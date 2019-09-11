using MvvmCross.Converters;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Localization;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android
{
    public class Setup<T> : MvxAppCompatSetup<T> where T : class, IMvxApplication, new()
    {
        protected override IMvxApplication CreateApp()
            => new App();

        //this is required, if you want to use app:MvxLang="Text WelcomeText"
        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);
            registry.AddOrOverwrite("Language", new MvxLanguageConverter());
        }
    }
}