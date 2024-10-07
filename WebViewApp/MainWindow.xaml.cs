using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebViewApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Initialized += MainWindow_Initialized;
            this.Loaded += MainWindow_Loaded;

            this.InitializeComponent();
        }

        private void MainWindow_Initialized(object? sender, EventArgs e)
        {
            InitializeWebView();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Set up environment options for logging
                var environment = await CoreWebView2Environment.CreateAsync(
                    null, // BrowserExecutableFolder (null means default)
                     @"D:\Source\Github\restricted-token-experiments\RestrictedAppLauncherV2\bin\Debug\net8.0-windows\WebViewApp", // UserDataFolder (null means default)
                    new CoreWebView2EnvironmentOptions("--enable-logging --v=1 --log-level=0"));

                var webView2 = (WebView2)FindName("WebView2");

                if (webView2 != null)
                {
                    //webView2.SetBinding(WebView2.SourceProperty, new Binding());

                    await webView2.EnsureCoreWebView2Async(environment);
                    webView2.Source = new Uri("https://www.microsoft.com");

                    // Hook the ProcessFailed event to catch crashes
                    webView2.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
            }
        }

        private void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            MessageBox.Show($"WebView2 process failed{Environment.NewLine}" +
                $"Failed kind: {e.ProcessFailedKind}{Environment.NewLine}" +
                $"Failed source module path: {e.FailureSourceModulePath}{Environment.NewLine}" +
                $"Process description: {e.ProcessDescription}{Environment.NewLine}" +
                $"Reason: {e.Reason}{Environment.NewLine}");
        }
    }
}