﻿using Mjml.Net.App.Wpf.Helpers;
using Mjml.Net.App.Wpf.HostObjects;
using System.Diagnostics;
using System.Windows;


namespace Mjml.Net.App.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await webviewMonacoEditor.EnsureCoreWebView2Async();
            await webviewPreview.EnsureCoreWebView2Async();
        }

        private async void webviewMonacoEditor_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            #if DEBUG
            webviewMonacoEditor.CoreWebView2.OpenDevToolsWindow();
            #endif

            // Register our bridge object to handle communication between WebView2 and CLR
            webviewMonacoEditor.CoreWebView2.AddHostObjectToScript("bridge", new TextEditorBridgeHostObject());

            // Register to events we're interested in
            TextEditorBridgeHostObject.OnDidChangeContent += TextEditorBridgeHostObject_OnDidChangeContent;
            
            // Load the editor page
            var htmlTemplateEditorPage = await ResourceHelpers.ReadResourceAsync("Mjml.Net.App.Wpf.Resources.TemplateEditorPage.html");
            webviewMonacoEditor.NavigateToString(htmlTemplateEditorPage);
        }
        private async void webviewPreview_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {

        }

        private void TextEditorBridgeHostObject_OnDidChangeContent(string modelContent)
        {
            Trace.WriteLine($"[TextEditor_OnDidChangeContent] OnEditorChangeDetected: {modelContent}");
        }      
    }
}
