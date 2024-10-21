using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Interpreter.Libraries
{
    internal class ConsoleLibrary : Library
    {
        public ConsoleLibrary()
        {
            avaiableFunctions = new List<string>()
            {
                "console.pause",
                "console.input",
                "console.key",
                "console.clear",
                "console.fgColor",
                "console.bgColor"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
                case "console.pause":
                case "pause":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    if (parameters.Length == 1) { Pause(parameters[0]); }
                    if (parameters.Length == 0) { Pause(null); }

                    result = null;
                    return true;

                case "console.input":
                case "input":
                    if (parameters.Length != 0)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }

                    result = GetUserInput();
                    return true;

                case "console.key":
                case "key":
                    if (parameters.Length != 0)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }

                    result = GetUserKey();
                    return true;

                case "console.clear":
                case "clear":
                    if (parameters.Length > 0)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 0);
                        break;
                    }
                    Console.Clear();

                    result = null;
                    return true;

                case "console.fgColor":
                case "fgColor":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    ChangeForegroundColor((string)parameters[0]);

                    result = null;
                    return true;

                case "console.bgColor":
                case "bgColor":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    ChangeBackgroundColor((string)parameters[0]);

                    result = null;
                    return true;
            }

            result = null;
            return false;
        }

        void Pause(object? message)
        {
            if (message != null)
            {
                Console.Write(message.ToString());
            }
            Console.ReadKey();
        }
        string? GetUserInput()
        {
            return Console.ReadLine();
        }
        ConsoleKey GetUserKey()
        {
            return Console.ReadKey().Key;
        }
        void ChangeForegroundColor(string color)
        {
            if (Enum.TryParse(typeof(ConsoleColor), color, true, out object? parsedColor))
            {
                Console.ForegroundColor = (ConsoleColor)parsedColor;
            }
            else
            {
                ColorNotFound(color);
            }
        }
        void ChangeBackgroundColor(string color)
        {
            if (Enum.TryParse(typeof(ConsoleColor), color, true, out object? parsedColor))
            {
                Console.BackgroundColor = (ConsoleColor)parsedColor;
            }
            else
            {
                ColorNotFound(color);
            }
        }

        #region Console Exceptions
        public void ColorNotFound(string color)
        {
            ExceptionsManager.PrintError(Init.currentLine, $"The \"{color}\" color wasn't found!");
        }
        #endregion
    }
}
