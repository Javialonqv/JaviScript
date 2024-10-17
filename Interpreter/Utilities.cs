using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal static class Utilities
    {
        public static string[] SplitWithSpaces(this string text)
        {
            string[]? splited = null;
            var chars = text.ToCharArray();
            bool isQuote = false;

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '"')
                {
                    isQuote = !isQuote;
                }
                if (chars[i] == ' ' && !isQuote) { chars[i] = '\0'; }
            }
            splited = new string(chars).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            return splited;
        }

        public static string RemoveWhitespaces(this string text)
        {
            string result = "";
            var chars = text.ToCharArray();
            bool isQuote = false;

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '"')
                {
                    isQuote = !isQuote;
                }
                if (chars[i] == ' ' && !isQuote) { chars[i] = '\0'; }
            }
            result = new string(chars).Replace("\0", "");
            return result;
        }
    }
}
