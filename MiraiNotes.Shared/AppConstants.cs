namespace MiraiNotes.Shared
{
    public static class AppConstants
    {
        public const string BaseGoogleApiUrl = "https://www.googleapis.com";
        public const string BaseGoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string BaseGoogleApprovalUrl = "https://accounts.google.com/o/oauth2/approval";

#if Android
        public const string ClientId = "xxx";
        public const string RedirectUrl = "xxx";
#else
        public const string ClientId = "xxx";
        public const string ClientSecret = "xxx";
        public const string RedirectUrl = "xxx";
#endif
    }
}