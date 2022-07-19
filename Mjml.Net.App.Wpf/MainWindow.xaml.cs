using Mjml.Net.App.Wpf.Helpers;
using Mjml.Net.App.Wpf.HostObjects;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace Mjml.Net.App.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MjmlRenderer MjmlRenderer { get; set; }
        public string DefaultMjmlTemplateString { get; set; }

        public MainWindow()
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
    }
}
