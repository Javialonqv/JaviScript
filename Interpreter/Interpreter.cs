using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using NCalc;
using Interpreter.Libraries;
using Windows.ApplicationModel.Background;

namespace Interpreter
{
    public enum BuiltInCommand
    {
        IMPORT,
        VAR
    }
    internal class Interpreter
    {
        public static bool ItsABuiltInCommand(string commandLine)
        {
            string[] splited = commandLine.SplitWithSpaces();

            return Enum.TryParse(typeof(BuiltInCommand), splited[0], true, out object? result);
        }

        public static dynamic GetValue(string text)
        {
            // Ver si es un valor nulo.
            if (text == "null" || text == "Null" || text == "undefined" || text == "Undefined")
            {
                return null;
            }

            // Intentar tratarlo como una operacion aritmética.
            object aritmeticOperationResult = AritmeticOperationOrConcatenation(text);
            if (aritmeticOperationResult != null) return aritmeticOperationResult;

            // Si es un string:
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Trim('\"');
            }

            // Si es un int
            if (int.TryParse(text, out int intResult))
            {
                return intResult;
            }

            // Si es un float.
            if (text.EndsWith("f"))
            {
                if (float.TryParse(text.Substring(0, text.Length - 2), out float floatResult))
                {
                    return floatResult;
                }
            }

            // Si de casualidad es otro comando.
            if (Enum.TryParse(typeof(BuiltInCommand), text, true, out object? commandResult))
            {
                string command = GetFunction(text);
                var parameters = GetFunctionParameters(text);
                Interpreter.ExecuteFunction(command, parameters, out object? result);
                return result;
            }

            // Ver si existe una variable con ese nombre.
            if (Program.variables.ContainsKey(text))
            {
                return Program.variables[text];
            }

            return text;
        }
        public static object AritmeticOperationOrConcatenation(string text)
        {
            // Checar si ejecutar si o no.
            bool execute = false;
            foreach (char c in text)
            {
                if (IsOperator(c)) execute = true;
            }
            if (!execute) return null;

            // Crear los "tokens".
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
                    //i++;
                }
                else
                {
                    currentToken += c;
                }
            }
            // Añadir el ultimo valor detectado si no está vacío.
            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(currentToken);
            }

            // Si es que hay parentesis tanto de abertura como de cierre, realizar las operaciones dentro de ellos primero.
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
                            tokens.Insert(i, result.ToString());
                            break;
                        }
                    }
                }
            }

            // Remover cualquier espacio en blanco dentro de los tokens.
            tokens.RemoveAll(t => string.IsNullOrWhiteSpace(t));

            // Realizar las operaciones recorriendo todos los tokens.
            dynamic finalResult = null;
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
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null;}
                    }
                    if (tokens[i] == "/")
                    {
                        try { finalResult /= GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null;}
                    }
                    if (tokens[i] == "*")
                    {
                        try { finalResult *= GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return null;}
                    }
                    i++;
                }
                else
                {
                    if (finalResult == null) finalResult = GetValue(tokens[i]);
                }
            }

            // Finalmente, retornar el valor final obtenido.
            return finalResult;
        }
        static bool IsOperator(char ch)
        {
            return ch == '+' || ch == '-' || ch == '/' || ch == '*';
        }

        public static BuiltInCommand GetBuiltItCommand(string commandLine)
        {
            string[] splited = commandLine.SplitWithSpaces();

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
        public static object[] GetBuiltInCommandParameters(string commandLine)
        {
            //object[] splited = commandLine.Split(" ").Skip(1).ToArray();

            object[] temp1 = commandLine.SplitWithSpaces().Skip(1).ToArray();
            string temp2 = string.Join("", temp1);
            object[] result = temp2.Split(",");

            return result.Select(obj => obj = GetValue(obj.ToString())).ToArray();
        }
        public static bool ExecuteCommand(BuiltInCommand commandType, object[] parameters)
        {
            return ExecuteCommand(commandType, parameters, out object? result);
        }
        public static bool ExecuteCommand(BuiltInCommand commandType, object[] parameters, out object? result)
        {
            switch (commandType)
            {
                case BuiltInCommand.VAR:
                    if (parameters.Length != 2)
                    {
                        ExceptionsManager.IncorrectCommandParametersNumber(commandType.ToString(), parameters.Length);
                        break;
                    }
                    Program.variables.Add(parameters[0].ToString(), parameters[1]);
                    result = null;
                    return true;
            }
            
            result = null;
            return false;
        }

        public static string GetFunction(string commandLine)
        {
            int parenthesisIndex = commandLine.IndexOf('(');
            int parenthesis2Index = commandLine.LastIndexOf(')');
            if (parenthesisIndex == -1 || parenthesis2Index == -1)
            {
                ExceptionsManager.NoFunctionParenthesisFound(commandLine);
                return "";
            }

            string command = commandLine.Substring(0, parenthesisIndex);
            return command;
        }
        public static object[] GetFunctionParameters(string commandLine)
        {
            List<object> parameters = new List<object>();
            int firstIndex = commandLine.IndexOf('(');
            int secondIndex = commandLine.LastIndexOf(')');
            if (firstIndex == -1 || secondIndex == -1)
            {
                return null;
            }

            string[] splitedParameters = commandLine.Substring(firstIndex + 1, secondIndex - firstIndex - 1).RemoveWhitespaces().Split(',');
            foreach (string parameter in splitedParameters)
            {
                //string toAdd = parameter.Substring(0, parameter.Length - 2);
                if (!string.IsNullOrEmpty(parameter)) parameters.Add(GetValue(parameter));
            }

            return parameters.ToArray();
        }
        public static bool ExecuteFunction(string function, object[] parameters)
        {
            return ExecuteFunction(function, parameters, out object? result);
        }
        public static bool ExecuteFunction(string function, object[] parameters, out object? result)
        {
            foreach (Library library in Program.loadedLibraries)
            {
                if (library.avaiableFunctions.Contains(function))
                {
                    return library.ExecuteFunction(function, parameters, out result);
                }
            }

            ExceptionsManager.FunctionNotFound(function);

            result = null;
            return false;
        }
    }
}
