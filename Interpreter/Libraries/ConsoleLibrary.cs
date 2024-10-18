using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                "console.clear",
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
    }
}
