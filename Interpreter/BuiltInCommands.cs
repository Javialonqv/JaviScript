using Interpreter.Libraries;
using System;
using System.Data;

namespace Interpreter
{
    public static class BuiltInCommands
    {
        // All the built-in libraries.
        static Dictionary<string, string> integratedLibraries = new Dictionary<string, string>()
        {
            { "console", "ConsoleLibrary" },
            { "convert", "ConvertLibrary" },
            { "internal", "InternalLibrary" }
        };

        public static bool Import(BuiltInCommand commandType, object[] parameters)
        {
            if (parameters.Length != 1)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "1");
                return false;
            }

            if (integratedLibraries.ContainsKey(parameters[0].ToString()))
            {
#pragma warning disable CS8600
                string realLibraryClassName = integratedLibraries[parameters[0].ToString()];
                Init.loadedLibraries.Add((Library)Utilities.CreateInstance(realLibraryClassName));
                return true;
            }
            else
            {
                ExceptionsManager.LibraryNotFound(parameters[0].ToString());
                return false;
            }

            return false;
        }

        public static bool Var(BuiltInCommand commandType, object[] parameters)
        {
            if (parameters.Length != 2)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "2");
                return false;
            }
            string funcName = parameters[0].ToString();
            if (Init.variables.Any(var => var.name == funcName) || Init.customFunctions.Any(func => func.name == funcName))
            {
                ExceptionsManager.VariableOrFunctionAlreadyDefined(funcName);
                return false;
            }
            Init.variables.Add(new Variable(parameters[0].ToString(), parameters[1]));

            return true;
        }

        public static bool Return(BuiltInCommand commandType, object[] parameters, out object? result)
        {
            if (parameters.Length != 1)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "1");
                result = null;
                return false;
            }

            result = parameters[0];
            return true;
        }

        public static bool If(BuiltInCommand commandType, object[] parameters)
        {
            if (parameters.Length > 1)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "2");
                return false;
            }

            Init.ifBlocks.Push((bool)parameters[0]);
            return true;
        }

        public static bool ElseIf(BuiltInCommand commandType, object[] parameters)
        {
            if (parameters.Length > 1)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "2");
                return false;
            }

            bool oldIfBlock = Init.ifBlocks.Pop();
            Init.ifBlocks.Push((bool)parameters[0] && !oldIfBlock);
            return true;
        }

        public static bool While(BuiltInCommand commandType, object[] parameters)
        {
            if (parameters.Length > 1)
            {
                ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length, "2");
                return false;
            }

            // Splits into spaces, then skips the first element.
            object[] splitedIntoSpaces = Init.fileLines[Init.realCurrentLine].SplitWithSpaces().Skip(1).ToArray();

            // Combine all this into a simple string.
            string combinedText = string.Join("", splitedIntoSpaces);

            // Finally, split again by commas.
            string[] result = combinedText.Split(",");

            Init.whileBlocks.Push(((bool)parameters[0], Init.realCurrentLine, result[0]));
            return true;
        }
    }
}