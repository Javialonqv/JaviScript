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
                "pause",
                "input"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
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

                case "input":
                    if (parameters.Length != 0)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }

                    result = GetUserInput();
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
