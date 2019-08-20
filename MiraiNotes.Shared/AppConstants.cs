namespace MiraiNotes.Shared
{
    public static class AppConstants
    {
        public const string BaseGoogleApiUrl = "https://www.googleapis.com";
        public const string BaseGoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string BaseGoogleApprovalUrl = "https://accounts.google.com/o/oauth2/approval";

#if Android
        public const string ClientId = "xxxx";
        public const string ClientSecret = "";
        public const string RedirectUrl = "xxxx";
#else
        public const string ClientId = "xxxx";
        public const string ClientSecret = "xxxx";
        public const string RedirectUrl = "xxxx";
#endif
    }
}