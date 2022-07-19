using Microsoft.Web.WebView2.Core;
using Mjml.Net.App.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Mjml.Net.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MjmlEditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            // WebView2 sends URLs that are navigated to in your application to the SmartScreen service, to ensure that your customers stay secure.
            // If you want to disable this navigation, you can do so via an environment variable:
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--disable-features=msSmartScreenProtection");

            // LR: Initialise WebView 2
            await webviewMonacoEditor.EnsureCoreWebView2Async();
            await webviewPreview.EnsureCoreWebView2Async();

            // LR: Prevent new windows being opened
            //webviewMonacoEditor.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            // LR: New Page Request
            //webviewMonacoEditor.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
        }

        private async void webviewMonacoEditor_CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
        {
            webviewMonacoEditor.CoreWebView2.OpenDevToolsWindow();

            var htmlTemplateEditorPage = await ResourceHelpers.ReadResourceAsync("Mjml.Net.App.Resources.TemplateEditorPage.html");

            webviewMonacoEditor.NavigateToString(htmlTemplateEditorPage);
        }

        private async void webviewPreview_CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
        {

        }
    }
}
