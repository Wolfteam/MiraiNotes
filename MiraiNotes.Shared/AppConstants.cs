using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MiraiNotes.Shared
{
    public static class AppConstants
    {
        public const string BaseGoogleApiUrl = "https://www.googleapis.com";
        public const string BaseGoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string BaseGoogleApprovalUrl = "https://accounts.google.com/o/oauth2/approval";

#if Android
        public const string ClientId = "xxxx";
        public const string ClientSecret = "xxxx";
        public const string RedirectUrl = "xxxx";
#else
        public const string ClientId = "xxxx";
        public const string ClientSecret = "xxxx";
        public const string RedirectUrl = "xxxx";
#endif

        public static IReadOnlyList<string> AppAccentColors => new List<string>
        {
            "#0077dd", "#008888", "#ee0088",
            "#cc4400", "#ee1122","#008899",
            "#118833","#881199", "#cc33bb",
            "#777777", "#ffb900", "#ff8c00",
            "#0063b1", "#6b69d6", "#68768a"
        };
    }
}