using Android.Widget;
using MiraiNotes.Android.Bindings;
using MvvmCross.Binding.Bindings.Target.Construction;
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

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            
            registry.RegisterCustomBindingFactory<ImageButton>(
                SubTaskButtonAnimateBinding.PropertyName,
                view => new SubTaskButtonAnimateBinding(view));

            registry.RegisterCustomBindingFactory<TextView>(
                StrikeThroughTextBinding.PropertyName,
                view => new StrikeThroughTextBinding(view));
        }
    }
}