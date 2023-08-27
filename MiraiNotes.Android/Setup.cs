using Android.Widget;
using Microsoft.Extensions.Logging;
using MiraiNotes.Android.Bindings;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.IoC;
using MvvmCross.Localization;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;
using Serilog.Extensions.Logging;

namespace MiraiNotes.Android
{
    public class Setup : MvxAndroidSetup<App>
    {
        protected override IMvxApplication CreateApp(IMvxIoCProvider iocProvider)
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

        protected override ILoggerProvider CreateLogProvider()
        {
            return new SerilogLoggerProvider();
        }

        protected override ILoggerFactory CreateLogFactory()
        {
            return new SerilogLoggerFactory();
        }
    }
}