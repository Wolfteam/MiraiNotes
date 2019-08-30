using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;
using System;

namespace MiraiNotes.Android.Common.Messages
{
    public class AccountChangeRequestMsg : MvxMessage
    {
        public bool DeleteAccount { get; private set; }
        public bool MarkAsActive { get; private set; }
        public GoogleUserViewModel Account { get; private set; }

        public AccountChangeRequestMsg(
            object sender,
            bool deleteAccount,
            bool markAsActive,
            GoogleUserViewModel account)
            : base(sender)
        {
            if (deleteAccount == markAsActive)
                throw new ArgumentOutOfRangeException(nameof(deleteAccount), deleteAccount, "You cannot delete and mark as active at the same time");

            Account = account;
            DeleteAccount = deleteAccount;
            MarkAsActive = markAsActive;
        }
    }
}