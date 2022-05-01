using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


namespace YaccLexTools.Utilities
{

    /// <summary>
    ///     Helper class to replace tokens in template files
    /// </summary>
    public class Template
    {
        private static readonly Regex _tokenRegex = new Regex(@"\$(?<tokenName>\w+)\$");

        /// <summary>
        ///     Calculate the result of applying tokens to a template
        /// </summary>
        /// <param name="input"> Template to be processed </param>
        /// <param name="tokens"> Values to be used for tokens </param>
        /// <returns> Template with tokens replaced </returns>
        public string Process(string input, IDictionary<string, string> tokens)
        {
            DebugCheck.NotEmpty(input);
            DebugCheck.NotNull(tokens);

            return _tokenRegex.Replace(
                input,
                match =>
                    {
                        var tokenName = match.Groups["tokenName"].Value;
                        var value = string.Empty;

                        tokens.TryGetValue(tokenName, out value);

                        return value;
                    });
        }


        public string GetContent(string templateName, IDictionary<string, string> tokens)
        {
            DebugCheck.NotEmpty(templateName);
            DebugCheck.NotNull(tokens);

            var stream = GetType().Assembly.GetManifestResourceStream("YaccLexTools.Templates." + templateName);
            Debug.Assert(stream != null);

            string content;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            return Process(content, tokens);
        }
    }
}
