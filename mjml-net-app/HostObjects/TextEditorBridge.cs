using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mjml.Net.App.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class TextEditorBridgeHostObject
    {
        //// Sample indexed property.
        //[System.Runtime.CompilerServices.IndexerName("Items")]
        //public string this[int index]
        //{
        //    get { return m_dictionary[index]; }
        //    set { m_dictionary[index] = value; }
        //}
        //private Dictionary<int, string> m_dictionary = new Dictionary<int, string>();

        public void OnEditorChangeDetected(string modelContent)
        {
            Trace.WriteLine($"[TextEditorBridge] OnEditorChangeDetected: {modelContent}");
        }
    }
}
