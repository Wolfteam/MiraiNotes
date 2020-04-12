using Android.Widget;
using MiraiNotes.Android.Bindings;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Localization;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android
{
    public class Setup : MvxAppCompatSetup<App>
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

            registry.RegisterCustomBindingFactory<LinearLayout>(
                IsSelectedBackgroundColorBinding.PropertyName,
                view => new IsSelectedBackgroundColorBinding(view));

            registry.RegisterCustomBindingFactory<MvxRecyclerView>(
                ExpandCollapseSubTasksAnimateBinding.PropertyName,
                view => new ExpandCollapseSubTasksAnimateBinding(view));
        }
    }
}