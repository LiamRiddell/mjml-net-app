using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mjml.Net.Editor.Helpers
{
    public static class ResourceHelpers
    {
        public static async Task<string> ReadResourceAsync(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();

            if (assembly == null)
                throw new Exception("Unable to get executing assembly");

            string resourcePath = name;

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

            if (resourcePath == null)
                throw new Exception($"Unable to find resource with name {resourcePath}");

            using Stream stream = assembly.GetManifestResourceStream(resourcePath);

            if (stream == null)
                throw new Exception($"Unable to find resource with name {name}");

            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }
}
