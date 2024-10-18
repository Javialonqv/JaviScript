using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using Interpreter.Libraries;
using Windows.ApplicationModel.Background;

#pragma warning disable CS8603
#pragma warning disable CS8602
namespace Interpreter
{
    // All the built-in commands.
    public enum BuiltInCommand
    {
        IMPORT,
        VAR,
        FUNC,
        RETURN,
        ENDFUNC
    }

    internal class Interpreter
    {
        // All the built-in libraries.
        static Dictionary<string, string> integratedLibraries = new Dictionary<string, string>()
        {
            { "console", "ConsoleLibrary" },
            { "convert", "ConvertLibrary" }
        };

        // To check if the specified command it's a buit-in one.
        public static bool ItsABuiltInCommand(string commandLine)
        {
            string[] splited = commandLine.SplitWithSpaces();

            return Enum.TryParse(typeof(BuiltInCommand), splited[0], true, out object? result);
        }

        // Try to convert the specified text into a value.
        public static dynamic GetValue(string text)
        {
            // Check if the value is null or undefined.
            if (text == "null" || text == "Null" || text == "undefined" || text == "Undefined")
            {
                return null;
            }

            // Try to check if it's an aritmetical operation.
            object aritmeticOperationResult = AritmeticOperationOrConcatenation(text);
            if (aritmeticOperationResult != null) return aritmeticOperationResult;

            // If it's a string.
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Trim('\"');
            }

            // If it's a int.
            if (int.TryParse(text, out int intResult))
            {
                return intResult;
            }

            // If it's a float.
            if (text.EndsWith("f"))
            {
                if (float.TryParse(text.Substring(0, text.Length - 2), out float floatResult))
                {
                    return floatResult;
                }
            }

            // If by any chance it's a function itself.
            string funcName = GetFunction(text, true);
            object[] funcParameters = GetFunctionParameters(text);
            if (ExecuteFunction(funcName, funcParameters, out object? result, true))
            {
                return result;
            }

            // Check if there's a variable with that name.
            if (Program.variables.ContainsKey(text))
            {
                return Program.variables[text];
            }

            // If nothing works, just return the same text WITHOUT modifications.
            return text;
        }
        // Tries to resolve the specified text as a aritmetical operation.
        public static object AritmeticOperationOrConcatenation(string text)
        {
            // Check if should execute or not.
            bool execute = false;
            foreach (char c in text)
            {
                if (IsOperator(c)) execute = true;
            }
            if (!execute) return null;

            #region Tokenize
            // Create the "tokens"
            text = text.RemoveWhitespaces();
            List<string> tokens = new List<string>();
            string currentToken = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (IsOperator(c) || c == '(' || c == ')')
                {
                    tokens.Add(currentToken);
                    currentToken = "";
                    tokens.Add(c.ToString());
                }
                else
                {
                    currentToken += c;
                }
            }
            // Add the last detected value if it's not empty.
            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(currentToken);
            }

            // If there are any parenthesis inside, solve them first.
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "(")
                {
                    for (int j = tokens.Count - 1; j >= 0; j--)
                    {
                        if (tokens[j] == ")")
                        {
                            string toPass = string.Join("", tokens.GetRange(i + 1, j - 1 - 3));
                            object result = AritmeticOperationOrConcatenation(toPass);
                            tokens.RemoveRange(i, j - i + 1);
#pragma warning disable CS8604
                            tokens.Insert(i, result.ToString());
                            break;
                        }
                    }
                }
            }
            #endregion

            // Remove any whitespace inside of the token list.
            tokens.RemoveAll(t => string.IsNullOrWhiteSpace(t));

            #region Resolve
            // Solve all the operations inside of the final tokens.
            dynamic? finalResult = null;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (string.IsNullOrEmpty(tokens[i])) continue;

                if (IsOperator(tokens[i].ToCharArray()[0]))
                {
                    if (tokens[i] == "+")
                    {
                        try { finalResult += GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null; }
                    }
                    if (tokens[i] == "-")
                    {
                        try { finalResult -= GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null; }
                    }
                    if (tokens[i] == "/")
                    {
                        try { finalResult /= GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null; }
                    }
                    if (tokens[i] == "*")
                    {
                        try { finalResult *= GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null; }
                    }
                    i++;
                }
                else
                {
                    if (finalResult == null) finalResult = GetValue(tokens[i]);
                }
            }
            #endregion

            // Finally, return the obtained result.
            return finalResult;
        }
        // Just check if the specified char is an operator LOL.
        static bool IsOperator(char ch)
        {
            return ch == '+' || ch == '-' || ch == '/' || ch == '*';
        }

        // Tries the specified command as an assigment one. 
        public static bool TryToAssign(string commandLine)
        {
            // Remove all the whitespaces in the command.
            commandLine = commandLine.RemoveWhitespaces();

            // The assigment commands ALWAYS have an equals character, ONLY ONE.
            if (commandLine.Count(ch => ch == '=') != 1) return false;
            string[] splited = commandLine.Split('=');

            // Get the variable name and the new value.
            string variableName = splited[0];
            string newValue = splited[1];

            // This only will work if the specified variable already exists and it's defined.
            if (Program.variables.ContainsKey(variableName))
            {
                Program.variables[variableName] = GetValue(newValue);
                return true;
            }
            else // Else, throw an error.
            {
                ExceptionsManager.UndefinedVariable(variableName);
                return false;
            }
        }
        // Gets the built-in command.
        public static BuiltInCommand GetBuiltItCommand(string commandLine)
        {
            // Splits the command into spaces.
            string[] splited = commandLine.SplitWithSpaces();

            // Try to parse the first element (the command itself) to the enum.
            if (Enum.TryParse(typeof(BuiltInCommand), splited[0], true, out object? result))
            {
                return (BuiltInCommand)result;
            }
            else
            {
                ExceptionsManager.BuiltInCommandNotFound(splited[0]);
                return default;
            }
        }
        // Gets the built-in command parameters.
        public static object[] GetBuiltInCommandParameters(string commandLine)
        {
            // Splits into spaces, then skips the first element.
            object[] temp1 = commandLine.SplitWithSpaces().Skip(1).ToArray();
            // Combine all this into a simple string.
            string temp2 = string.Join("", temp1);
            // Finally, split again by commas.
            object[] result = temp2.Split(",");

            // Return the result, but before that, convert the value for each string.
            return result.Select(obj => obj = GetValue(obj.ToString())).ToArray();
        }
        public static bool ExecuteCommand(BuiltInCommand commandType, object[] parameters)
        {
            return ExecuteCommand(commandType, parameters, out object? result);
        }
        // Executes the specified built-in command.
        public static bool ExecuteCommand(BuiltInCommand commandType, object[] parameters, out object? result)
        {
            // FUNC and ENDFUNC aren´t here because they are managed in the Program class.
            switch (commandType)
            {
                case BuiltInCommand.IMPORT:
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length);
                        break;
                    }

                    if (integratedLibraries.ContainsKey(parameters[0].ToString()))
                    {
#pragma warning disable CS8600
                        string realLibraryClassName = integratedLibraries[parameters[0].ToString()];
                        Program.loadedLibraries.Add((Library)Utilities.CreateInstance(realLibraryClassName));
                    }
                    else
                    {
                        ExceptionsManager.LibraryNotFound(parameters[0].ToString());
                        break;
                    }
                    result = null;
                    return true;

                case BuiltInCommand.VAR:
                    if (parameters.Length != 2)
                    {
                        ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length);
                        break;
                    }
                    Program.variables.Add(parameters[0].ToString(), parameters[1]);
                    result = null;
                    return true;

                case BuiltInCommand.RETURN:
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length);
                        break;
                    }
                    result = parameters[0];
                    return true;
            }
            
            result = null;
            return false;
        }

        // Gets the specified function's name.
        public static string GetFunction(string commandLine, bool skipErrors = false)
        {
            // Gets the parenthesis indexes.
            int parenthesisIndex = commandLine.IndexOf('(');
            int parenthesis2Index = commandLine.LastIndexOf(')');
            // A function ALWAYS contains the TWO parenthesis.
            if (parenthesisIndex == -1 || parenthesis2Index == -1)
            {
                if (!skipErrors) ExceptionsManager.NoFunctionParenthesisFound(commandLine);
                return "";
            }

            // Return the function without the parenthesis.
            string command = commandLine.Substring(0, parenthesisIndex);
            return command;
        }
        // Gets the specified function's parameters.
        public static object[] GetFunctionParameters(string commandLine, bool isFromACustomFunction = false)
        {
            // Gets the parenthesis indexes.
            List<object> parameters = new List<object>();
            int firstIndex = commandLine.IndexOf('(');
            int secondIndex = commandLine.LastIndexOf(')');
            // A function ALWAYS contains the TWO parenthesis.
            if (firstIndex == -1 || secondIndex == -1)
            {
                return null;
            }

            // Get all the content inside of the parenthesis, remove whitespaces and split by commas.
            string[] splitedParameters = commandLine.Substring(firstIndex + 1, secondIndex - firstIndex - 1).RemoveWhitespaces().Split(',');
            // Foreach parameter, get it's real value.
            foreach (string parameter in splitedParameters)
            {
                //string toAdd = parameter.Substring(0, parameter.Length - 2);
                if (!string.IsNullOrEmpty(parameter) && !isFromACustomFunction) parameters.Add(GetValue(parameter));
                if (!string.IsNullOrEmpty(parameter) && isFromACustomFunction) parameters.Add(parameter);
            }

            // Do I really need to explain what this thing does? LOL
            return parameters.ToArray();
        }
        public static bool ExecuteFunction(string function, object[] parameters)
        {
            return ExecuteFunction(function, parameters, out object? result);
        }
        // Executes the specified function.
        public static bool ExecuteFunction(string function, object[] parameters, out object? result, bool skipErrors = false)
        {
            result = null;

            // First check if the function is a custom one, if that's the case, this should be true.
            if (Program.customFunctions.Any(func => func.name == function && func.parameters.Count == parameters.Length))
            {
                // Get the function.
                CustomFunction func = Program.customFunctions.Find(func => func.name == function);

                // First iterate foreach parameter and add them as a new variable.
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    Program.variables.Add(func.parameters[i], parameters[i]);
                }

                // Foreach line inside of the function block, execute the commands.
                for (int i = func.startIndex + 1; i < Program.fileLines.Length; i++)
                {
                    if (Program.fileLines[i] == "EndFunc") break;
                    if (Program.ExecuteCommand(Program.fileLines[i], out object? customFuncResult, true))
                    {
                        if (GetBuiltItCommand(Program.fileLines[i]) == BuiltInCommand.RETURN)
                        {
                            result = customFuncResult;
                        }
                    }
                }

                // Delete the functions variables.
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    Program.variables.Remove(func.parameters[i]);
                }

                // The function was executed successfully.
                //result = null;
                return true;
            }

            // If not, check in the built-in libraries that are LOADED.
            foreach (Library library in Program.loadedLibraries)
            {
                if (library.avaiableFunctions.Contains(function)) // If there's a library that contains the specified function WITH THE FULL NAME.
                {
                    bool temp = library.ExecuteFunction(function, parameters, out result);
                    return temp; // Execute it.
                }
                // If there's a library that contains the specified function WITH THE SHORT NAME.
                else if (library.avaiableFunctions.Any(str => str.Substring(str.IndexOf('.') + 1) == function))
                {
                    bool temp = library.ExecuteFunction(function, parameters, out result);
                    return temp; // Execute it.
                }
            }

            // If nothing works, throw an error and return false.
            if (!skipErrors) ExceptionsManager.FunctionNotFound(function);

            result = null;
            return false;
        }
    }
}
