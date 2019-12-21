using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MiraiNotes.UWP.Controls.Authentication
{
    public sealed class CustomWebAuthenticationBroker
    {
        private static Uri redirectUri;
        private static ContentDialog dialog;
        private static string code = string.Empty;
        private static uint errorCode = 0;

        private static TextBox _address;

        public static Task<CustomWebAuthenticationResult> AuthenticateAsync(
            WebAuthenticationOptions options,
            Uri requestUri,
            bool showUrl)
            => AuthenticateAsync(options, requestUri, WebAuthenticationBroker.GetCurrentApplicationCallbackUri(), showUrl);


        public static async Task<CustomWebAuthenticationResult> AuthenticateAsync(
            WebAuthenticationOptions options,
            Uri requestUri,
            Uri callbackUri,
            bool showUrl)
        {
            if (options != WebAuthenticationOptions.None)
                throw new ArgumentException("WebAuthenticationBroker currently only supports WebAuthenticationOptions.None", "options");

            redirectUri = callbackUri;
            dialog = new ContentDialog();

            var grid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = "Connect to a service",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0)
            };
            grid.Children.Add(label);

            var closeButton = new Button
            {
                Content = "",
                FontFamily = new FontFamily("Segoe UI Symbol"),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            closeButton.Click += (s, e) => { dialog.Hide(); };
            grid.Children.Add(closeButton);

            _address = new TextBox
            {
                Text = string.Empty,
                Margin = new Thickness(0, 5, 0, 5),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
            };
            _address.SetValue(Grid.RowProperty, 1);

            if (showUrl)
            {
                grid.Children.Add(_address);

                var splitter = new GridSplitter();
                splitter.SetValue(Grid.RowProperty, 2);
                splitter.Element = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsHitTestVisible = false,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "\uE76F",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Segoe MDL2 Assets")
                };
                grid.Children.Add(splitter);
            }

            var webView = new WebView(WebViewExecutionMode.SameThread) { Source = requestUri };
            webView.NavigationStarting += WebView_NavigationStarting;
            webView.NavigationFailed += WebView_NavigationFailed;
            webView.ContentLoading += (sender, args) =>
            {
                _address.Text = args.Uri.ToString();
            };

            webView.MinWidth = 460;
            webView.MinHeight = 600;

            var scrollViewer = new ScrollViewer
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = webView
            };
            scrollViewer.SetValue(Grid.RowProperty, 3);

            grid.Children.Add(scrollViewer);
            dialog.Content = grid;

            //dialog.GotFocus += (s, e) => { webView.Focus(Windows.UI.Xaml.FocusState.Programmatic); };
            var res = await dialog.ShowAsync();
            return new CustomWebAuthenticationResult(code, errorCode, errorCode > 0 ? CustomWebAuthenticationStatus.ErrorHttp : string.IsNullOrEmpty(code) ? CustomWebAuthenticationStatus.UserCancel : CustomWebAuthenticationStatus.Success);
        }

        private static void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            errorCode = (uint)e.WebErrorStatus;
            dialog.Hide();
        }

        private static void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.ToString().StartsWith(redirectUri.ToString()))
            {
                code = args.Uri.Query;

                args.Cancel = true;
                dialog.Hide();
            }
        }
    }
}
