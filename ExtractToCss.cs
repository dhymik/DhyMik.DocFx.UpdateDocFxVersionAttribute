using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DhyMik.DocFx
{
    /// <summary>
    /// Helper classes for <see cref="DhyMik.DocFx.UpdateDocFxVersionAttributeTask"/>.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Extension method on <see cref="Dictionary{string, string}"/>.
        /// Reads all elements from dictionary and creates a css string with
        /// css variables.
        /// </summary>
        internal static string ExtractToCss(this Dictionary<string, string> globalMetaData)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(":root {\n");
            foreach (var attribute in globalMetaData)
            {
                sb.Append($@"    --{attribute.Key}: ""{attribute.Value}"";");
                sb.Append("\n");
            }
            sb.Append("}\n");

            return sb.ToString();
        }
    }
}
