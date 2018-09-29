﻿using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class MiscellaneousUtils
    {
        /// <summary>
        /// Gets the app version duh!
        /// </summary>
        /// <returns>App version (e.g: 1.0.2.0)</returns>
        public static string GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
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
                frameworkElement.RequestedTheme = (ElementTheme)appTheme;
        }

        public static Color GetColor(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length > 6)
            {
                byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                var r = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
        }

        public static Color GetSystemAccentColor()
            => new UISettings().GetColorValue(UIColorType.Accent);
    }
}