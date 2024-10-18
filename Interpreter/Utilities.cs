using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal static class Utilities
    {
        public const string validCharacters = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";

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

        public static object? CreateInstance(string className)
        {
            Type? type = Type.GetType("Interpreter.Libraries." + className);

#pragma warning disable CS8604
            return Activator.CreateInstance(type);
        }

        public static bool IsNumber(object obj)
        {
            return obj is int || obj is float;
        }

        public static bool ValidFunctionName(string functionName)
        {
            functionName = functionName.ToUpper();

            foreach (char c in functionName)
            {
                if (!validCharacters.Contains(c)) return false;
            }

            return true;
        }
    }
}
