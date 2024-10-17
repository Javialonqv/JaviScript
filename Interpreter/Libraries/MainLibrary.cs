using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Libraries
{
    public class MainLibrary : Library
    {
        public MainLibrary()
        {
            avaiableFunctions = new ()
            {
                "print"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
                case "print":
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, parameters.Length);
                        break;
                    }
                    Print(parameters[0]);
                    result = null;
                    return true;
            }

            result = null;
            return false;
        }

        void Print(object message)
        {
            Console.Write(message);
        }
    }
}
