using System.Collections.Generic;

namespace MiraiNotes.Shared
{
    public static class AppConstants
    {
        public const string BaseGoogleApiUrl = "https://www.googleapis.com";
        public const string BaseGoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string BaseGoogleApprovalUrl = "https://accounts.google.com/o/oauth2/approval";

        //Accent colors
        public const string AccentColorLightBlue = "#0077dd";
        public const string AccentColorLimeGreen = "#1be556";
        public const string AccentColorPink = "#ee0088";
        public const string AccentColorDarkOrange = "#cc4400";
        public const string AccentColorVividRed = "#ee1122";
        public const string AccentColorDarkCyan = "#008899";
        public const string AccentColorDarkGreen = "#118833";
        public const string AccentColorDarkMagenta = "#881199";
        public const string AccentColorMagenta = "#ff00e3";
        public const string AccentColorDarkGray = "#777777";
        public const string AccentColorOrange = "#ffb900";
        public const string AccentColorYellow = "#f5ff00";
        public const string AccentColorDarkBlue = "#0063b1";
        public const string AccentColorViolet = "#8600ff";
        public const string AccentColorLightGrey = "#a8a9aa";

        public static IReadOnlyList<string> AppAccentColors => new List<string>
        {
            AccentColorLightBlue, AccentColorLimeGreen, AccentColorPink,
            AccentColorDarkOrange, AccentColorVividRed, AccentColorDarkCyan,
            AccentColorDarkGreen, AccentColorDarkMagenta, AccentColorMagenta,
            AccentColorDarkGray, AccentColorOrange, AccentColorYellow,
            AccentColorDarkBlue, AccentColorViolet, AccentColorLightGrey
        };
    }
}