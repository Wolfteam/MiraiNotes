using Android.Content;
using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class TasksMenuOptionsDialogFragment : BaseBottomSheetDialogFragment<TaskMenuOptionsViewModel>
    {
        private IMvxInteraction<TaskItemViewModel> _shareTaskRequest;

        public IMvxInteraction<TaskItemViewModel> ShareTaskRequest
        {
            get => _shareTaskRequest;
            set
            {
                if (_shareTaskRequest != null)
                    _shareTaskRequest.Requested -= ShareTask;

                _shareTaskRequest = value;
                _shareTaskRequest.Requested += ShareTask;
            }
        }

        public override int LayoutId
            => Resource.Layout.TaskMenuOptionsDialog;


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var set = this.CreateBindingSet<TasksMenuOptionsDialogFragment, TaskMenuOptionsViewModel>();
            set.Bind(this).For(v => v.ShareTaskRequest).To(vm => vm.ShareTask);
            set.Apply();

            return view;
        }

        private void ShareTask(object sender, MvxValueEventArgs<TaskItemViewModel> args)
        {
            var task = args.Value;
            var sendIntent = new Intent();
            sendIntent.SetAction(Intent.ActionSend);
            sendIntent.PutExtra(Intent.ExtraText, task.Notes);
            sendIntent.SetType("text/plain");

            Intent shareIntent = Intent.CreateChooser(sendIntent, task.Title);
            StartActivity(shareIntent);
        }
    }
}