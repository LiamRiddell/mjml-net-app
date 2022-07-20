using Mjml.Net;
using Mjml.Net.Editor.Helpers;
using Mjml.Net.Editor.HostObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mjml.Net.Editor.Pages
{
    /// <summary>
    /// Interaction logic for MainEditor.xaml
    /// </summary>
    public partial class MainEditorPage : Page
    {
        public MjmlRenderer MjmlRenderer { get; set; }
        public string DefaultMjmlTemplateString { get; set; }
        public bool ShowHtml { get; set; } = false;

        public MainEditorPage()
        {
            InitializeComponent();
            MjmlRenderer = new MjmlRenderer();
            DefaultMjmlTemplateString = @"
<mjml>
   <mj-body>
       <mj-section>
           <mj-column>
               <mj-image width=""100px"" src=""https://mjml.io/assets/img/logo-small.png""></mj-image>
               <mj-divider border-color=""#F45E43""></mj-divider>
               <mj-text font-size=""20px"" color=""#F45E43"" font-family=""helvetica"">Hello World</mj-text>
           </mj-column>
       </mj-section>
   </mj-body>
</mjml>    
            ";
        }

        private async void Page_Initialized(object sender, EventArgs e)
        {
            await webviewMonacoEditor.EnsureCoreWebView2Async();
            await webviewPreview.EnsureCoreWebView2Async();
           // await webviewHtml.EnsureCoreWebView2Async();
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
            var htmlTemplateEditorPage = await ResourceHelpers.ReadResourceAsync("Mjml.Net.Editor.Resources.TemplateEditorPage.html");
            webviewMonacoEditor.NavigateToString(htmlTemplateEditorPage);
        }

        private async void webviewHtml_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
#if DEBUG
            //webviewHtml.CoreWebView2.OpenDevToolsWindow();
#endif
            // Load the editor page
            // var htmlTemplateEditorPage = await ResourceHelpers.ReadResourceAsync("Mjml.Net.Editor.Resources.HtmlPreviewPage.html");
            // webviewHtml.NavigateToString(htmlTemplateEditorPage);
        }

        private async void webviewPreview_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            // DefaultMjmlTemplateString
#if DEBUG
            webviewPreview.CoreWebView2.OpenDevToolsWindow();
#endif

            // Compile the default template
            if (TryCompileMjmlTemplateAsync(DefaultMjmlTemplateString, out string htmlContent))
            {
                webviewPreview.NavigateToString(htmlContent);
                htmlPreviewBox.Text = htmlContent;
            }
        }
        
        private async void TextEditorBridgeHostObject_OnDidChangeContent(string modelContent)
        {
            Trace.WriteLine($"[TextEditor_OnDidChangeContent] OnEditorChangeDetected: {modelContent}");

            if (TryCompileMjmlTemplateAsync(modelContent, out string htmlContent))
            {
                // webviewPreview.NavigateToString(htmlContent);
                await webviewPreview.ExecuteScriptAsync($"document.body.innerHTML = `{htmlContent.Replace("`", "``")}`;");
                // await webviewHtml.CoreWebView2.ExecuteScriptAsync($"window.monacoEditorInstance.getModel().setValue('{htmlContent}')");
                htmlPreviewBox.Text = htmlContent;
            }
            else
            {
                await webviewPreview.ExecuteScriptAsync($"document.body.innerHTML = ``;");
                htmlPreviewBox.Text = string.Empty;
            }
        }
        
        private bool TryCompileMjmlTemplateAsync(string mjmlTemplate, out string htmlContent)
        {
            htmlContent = string.Empty;

            if (MjmlRenderer == null)
                return false;

            try
            {
                mjmlTemplate = MjmlRenderer.FixXML(mjmlTemplate);

                var result = MjmlRenderer.Render(mjmlTemplate);

                if (result.Errors.Any())
                {
                    return false;
                }

                htmlContent = result.Html;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void MenuItemOpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (webviewMonacoEditor.IsInitialized)
            {
                Microsoft.Win32.OpenFileDialog dlg = new()
                {
                    FileName = "MjmlTemplate",
                    DefaultExt = ".mjml",
                    Filter = "Mjml Templates (.mjml)|*.mjml"
                };

                var result = dlg.ShowDialog();

                if (result == true)
                {
                    if (!string.IsNullOrEmpty(dlg.FileName))
                    {
                        var mjmlTemplateText = await File.ReadAllTextAsync(dlg.FileName);

                        if (!string.IsNullOrEmpty(mjmlTemplateText))
                        { 
                            await webviewMonacoEditor.ExecuteScriptAsync($"monacoEditorInstance.getModel().setValue(`{mjmlTemplateText}`);");
                        }
                    }
                }

            }
        }

        private async void MenuItemNewTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (webviewMonacoEditor.IsInitialized)
            {
                await webviewMonacoEditor.ExecuteScriptAsync($"monacoEditorInstance.getModel().setValue(``);");
            }
        }

        private async void MenuItemSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            // Get the source code from monaco editor
            var modelContentEscaped = await webviewMonacoEditor.ExecuteScriptAsync(@"(function() { return monacoEditorInstance.getModel().getValue() })();");
            modelContentEscaped = Regex.Unescape(modelContentEscaped).TrimStart('"').TrimEnd('"');

            if (webviewMonacoEditor.IsInitialized && !string.IsNullOrEmpty(modelContentEscaped))
            {
                Microsoft.Win32.SaveFileDialog dlg = new()
                {
                    FileName = "MjmlTemplate",
                    DefaultExt = ".mjml",
                    Filter = "Mjml Templates (.mjml)|*.mjml"
                };

                var result = dlg.ShowDialog();

                if (result == true)
                {
                    if (!string.IsNullOrEmpty(dlg.FileName))
                    {
                        await File.WriteAllTextAsync(dlg.FileName, modelContentEscaped);
                    }
                }
            }
        }

        private async void MenuItemShowHtml_Checked(object sender, RoutedEventArgs e)
        {
            if (webviewPreview != null && htmlPreviewBox != null)
            {
                webviewPreview.Visibility = Visibility.Collapsed;
                htmlPreviewBox.Visibility = Visibility.Visible;
            }
        }

        private async void MenuItemShowHtml_Unchecked(object sender, RoutedEventArgs e)
        {
            if (webviewPreview != null && htmlPreviewBox != null)
            {
                webviewPreview.Visibility = Visibility.Visible;
                htmlPreviewBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}
