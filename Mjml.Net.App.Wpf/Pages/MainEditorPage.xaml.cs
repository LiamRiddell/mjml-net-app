using Mjml.Net;
using Mjml.Net.Editor.Helpers;
using Mjml.Net.Editor.HostObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            }
        }
        private async void TextEditorBridgeHostObject_OnDidChangeContent(string modelContent)
        {
            Trace.WriteLine($"[TextEditor_OnDidChangeContent] OnEditorChangeDetected: {modelContent}");

            if (TryCompileMjmlTemplateAsync(modelContent, out string htmlContent))
            {
                webviewPreview.NavigateToString(htmlContent);
            }
        }
        private bool TryCompileMjmlTemplateAsync(string mjmlTemplate, out string htmlContent)
        {
            htmlContent = string.Empty;

            if (MjmlRenderer == null)
                return false;

            mjmlTemplate = MjmlRenderer.FixXML(mjmlTemplate);

            var result = MjmlRenderer.Render(mjmlTemplate);

            if (result.Errors.Any())
            {
                return false;
            }

            htmlContent = result.Html;
            return true;
        }

        private void webviewHtml_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {

        }
    }
}
