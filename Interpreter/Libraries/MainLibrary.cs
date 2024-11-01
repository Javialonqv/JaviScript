﻿using System;
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
                "main.print",
                "main.printl",
                "main.exit"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
                case "main.print":
                case "print":
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, parameters.Length, "1");
                        break;
                    }
                    Print(parameters[0], false);
                    result = null;
                    return true;

                case "main.printl":
                case "printl":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, parameters.Length, "1");
                        break;
                    }
                    if (parameters.Length == 1) { Print(parameters[0], true); }
                    if (parameters.Length == 0) { Print("", true); }
                    result = null;
                    return true;

                case "main.exit":
                case "exit":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, parameters.Length, "0 or 1");
                        break;
                    }
                    if (parameters.Length == 1)
                    {
                        if (Utilities.IsNumber(parameters[0]))
                        {
                            Exit((int)parameters[0]);
                        }
                        else
                        {
                            ExceptionsManager.InvalidFunctionParameterType(command, 0, parameters[0].GetType().Name, "Int");
                        }
                    }
                    if (parameters.Length == 0) { Exit(0); }
                    result = null;
                    return true;
            }

            result = null;
            return false;
        }

        void Print(object message, bool printLine)
        {
            Console.Write(message);
            if (printLine) { Console.WriteLine(); }
        }

        void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
    }
}
