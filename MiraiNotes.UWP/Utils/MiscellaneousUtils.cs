using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MiraiNotes.UWP.Utils
{
    public static class MiscellaneousUtils
    {
        const string USER_IMAGE_FILE_NAME = "user_image.png";

        /// <summary>
        /// Gets the app version duh!
        /// </summary>
        /// <returns>App version (e.g: 1.0.2.0)</returns>
        public static string GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;
            string appVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
#if DEBUG
            return $"{appVersion} - DEBUG";
#else
            return appVersion;
#endif
        }

        /// <summary>
        /// Gets the path to the app local folder
        /// </summary>
        /// <returns>The app local folder path</returns>
        public static string GetApplicationPath()
            => ApplicationData.Current.LocalFolder.Path;

        /// <summary>
        /// Removes the indicated file from the app local folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns><see cref="Task"/></returns>
        public static async Task RemoveFile(string filename)
        {
            try
            {
                await (await ApplicationData.Current.LocalFolder.GetFileAsync(filename))
                    .DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
                //no file
            }
        }

        /// <summary>
        /// Saves the file to the app local folder
        /// </summary>
        /// <param name="filename">Filename (e.g: yolo.png)</param>
        /// <param name="fileBytes">Bytes to save</param>
        /// <returns><see cref="Task"/></returns>
        public static async Task SaveFile(string filename, byte[] fileBytes)
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var thumb = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                var fs = await thumb.OpenStreamForWriteAsync(); //get stream
                var writer = new DataWriter(fs.AsOutputStream());

                writer.WriteBytes(fileBytes); //write
                await writer.StoreAsync();
                await writer.FlushAsync();

                writer.Dispose();
            }
            catch (Exception)
            {
                //Could save the file
            }
            await Task.Delay(2000);
        }

        /// <summary>
        /// Changes the app theme and the accent color
        /// </summary>
        /// <param name="appTheme">App theme</param>
        /// <param name="hexAccentColor">Accent color in hex</param>
        public static void ChangeCurrentTheme(AppThemeType appTheme, string hexAccentColor)
        {
            Color accentColor;
            if (!string.IsNullOrEmpty(hexAccentColor))
                accentColor = GetColor(hexAccentColor);
            else
                accentColor = GetSystemAccentColor();

            var tb = ApplicationView.GetForCurrentView().TitleBar;
            tb.BackgroundColor =
                tb.ButtonBackgroundColor =
                    tb.InactiveBackgroundColor =
                          tb.ButtonInactiveBackgroundColor = accentColor;

            var systemColors = new List<string>
            {
               "SystemAccentColor","SystemColorHighlightColor", "SystemAccentColorLight2",
               "SystemAccentColorLight3","SystemAccentColorDark2", "SystemAccentColorDark3"
            };
            foreach (var systemColor in systemColors)
                Application.Current.Resources[systemColor] = accentColor;


            var brush = new SolidColorBrush(accentColor);
            (Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = accentColor;
            (Application.Current.Resources["SystemControlHighlightListAccentLowBrush"] as SolidColorBrush).Color = accentColor;
            Application.Current.Resources["SystemControlHighlightListAccentHighBrush"] = brush;
            Application.Current.Resources["NavigationViewSelectionIndicatorForeground"] = brush;
            //Application.Current.Resources["SystemControlHighlightAltBaseHighBrush"] = accentColor;
            //Application.Current.Resources["SystemControlForegroundBaseHighBrush"] = accentColor;
            //Application.Current.Resources["SystemControlHighlightListMediumBrush"] = brush;
            //Application.Current.Resources["SystemControlHighlightListLowBrush"] = brush;
            //(Application.Current.Resources["SystemControlHighlightAccent3RevealAccent2BackgroundBrush"] as RevealBackgroundBrush).Color = accentColor;
            //(Application.Current.Resources["SystemControlHighlightAccent3RevealBackgroundBrush"] as RevealBackgroundBrush).Color = accentColor;
            //(Application.Current.Resources["SystemControlHighlightAccent2RevealBackgroundBrush"] as RevealBackgroundBrush).Color = accentColor;

            // Set theme for window root.
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = (ElementTheme)appTheme;
            }
        }

        public static Color GetColor(string hex)
        {
            var color = ColorUtil.ToColor(hex);

            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color GetSystemAccentColor()
            => new UISettings().GetColorValue(UIColorType.Accent);

        public static string GetUserProfileImagePath(string id)
            => Path.Combine(GetApplicationPath(), BuildImageFilename(id));

        public static string BuildImageFilename(string id)
            => $"{id}_{USER_IMAGE_FILE_NAME}";

        public static async Task DownloadProfileImage(string url, string googleUserId)
        {
            string filename = BuildImageFilename(googleUserId);
            try
            {
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(url);
                    await SaveFile(filename, imageBytes);
                }
            }
            catch (Exception)
            {
                //Http excep..
            }
        }

        public static T FindControl<T>(UIElement parent, string ControlName) where
            T : FrameworkElement
        {
            if (parent == null)
                return null;
            if (parent.GetType() == typeof(T) && ((T)parent).Name == ControlName)
            {
                return (T)parent;
            }
            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, ControlName) != null)
                {
                    result = FindControl<T>(child, ControlName);
                    break;
                }
            }
            return result;
        }

        public static string GetLogsPath()
            => Path.Combine(GetApplicationPath(), "Logs");
    }
}
