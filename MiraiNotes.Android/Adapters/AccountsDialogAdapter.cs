using Android.App;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels;
using System.Collections.Generic;

namespace MiraiNotes.Android.Adapters
{
    public class AccountsDialogAdapter : BaseAdapter<GoogleUserViewModel>
    {
        private readonly Activity _context;
        private List<GoogleUserViewModel> Items { get; }

        public override GoogleUserViewModel this[int position] => Items[position];

        public override int Count => Items.Count;

        public AccountsDialogAdapter(Activity context, List<GoogleUserViewModel> items)
        {
            _context = context;
            Items = items;
        }

        public override long GetItemId(int position)
            => Items[position].Id;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //var view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.AccountItem, null);

            //var vm = Items[position];

            //var accountImg = view.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.AccountImg);
            //var accountName = view.FindViewById<TextView>(Resource.Id.AccountName);
            //var accountEmail = view.FindViewById<TextView>(Resource.Id.AccountEmail);
            //var changeAccount = view.FindViewById<ImageButton>(Resource.Id.ChangeAccount);
            //var deleteAccount = view.FindViewById<ImageButton>(Resource.Id.DeleteAccount);

            //using (var img = MiscellaneousUtils.GetImageBitmap(vm.PictureUrl))
            //{
            //    if (img != null)
            //        accountImg.SetImageBitmap(img);
            //}

            //accountName.Text = vm.Fullname;
            //accountEmail.Text = vm.Email;
            //changeAccount.Enabled = !vm.IsActive;
            //changeAccount.Visibility = !vm.IsActive ? ViewStates.Visible : ViewStates.Gone;
            //changeAccount.Click += (sender, args) => vm.ChangeCurrentAccountCommand.Execute();
            //deleteAccount.Click += (sender, args) => vm.DeleteAccountCommand.Execute();

            //return view;
            return convertView;
        }
    }
}