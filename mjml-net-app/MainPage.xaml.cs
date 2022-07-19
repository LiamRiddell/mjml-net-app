using Microsoft.Web.WebView2.Core;
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

            webviewMonacoEditor.NavigateToString(@"
                <!DOCTYPE html>
                <html lang=""en"">

                <head>
                    <meta charset=""UTF-8"">
                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>mjml-net-app</title>
                    <style>
                        html,
                        body,
                        #container {
                            position: absolute;
                            left: 0;
                            top: 0;
                            width: 100%;
                            height: 100%;
                            margin: 0;
                            padding: 0;
                            overflow: hidden;
                        }
                    </style>
                </head>

                <body>
                    <script src=""https://unpkg.com/monaco-editor@latest/min/vs/loader.js""></script>
                    <div id=""container""></div>
                    <script>
                        require.config({ paths: { 'vs': 'https://unpkg.com/monaco-editor@latest/min/vs' } });
                        window.MonacoEnvironment = { getWorkerUrl: () => proxy };

                        let proxy = URL.createObjectURL(new Blob([`
                            self.MonacoEnvironment = {
                                baseUrl: 'https://unpkg.com/monaco-editor@latest/min/'
                            };
                            importScripts('https://unpkg.com/monaco-editor@latest/min/vs/base/worker/workerMain.js');
                        `], { type: 'text/javascript' }));

                        require([""vs/editor/editor.main""], function () {
                            let editor = monaco.editor.create(document.getElementById('container'), {
                                value: [
                                    'function x() {',
                                    '\tconsole.log(""Hello world!"");',
                                    '}'
                                ].join('\n'),
                                language: 'javascript',
                                theme: 'vs-dark'
                            });
                        });
                    </script>
                </body>
                </html>
            ");
        }

        private async void webviewPreview_CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
        {

        }
    }
}
