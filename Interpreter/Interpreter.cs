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
        ENDFUNC,
        IF,
        ELSE,
        ELSEIF,
        ENDIF
    }

    internal class Interpreter
    {
        // Info to create custom functions.
        static string funcName = "";
        static List<string> funcParameters = new List<string>();
        static int funcStartIndex = 0;
        static bool funcReturnsSomething = false;

        public static bool ExecuteLine(string line, bool executeFunc = false)
        {
            return ExecuteLine(line, out object? result, executeFunc);
        }
        public static bool ExecuteLine(string line, out object? result, bool executeFunc = false)
        {
            result = null;

            // Those lines that starts with '#' are comments, ignore them.
            if (line.StartsWith("# ")) return false;

            // If the line is null of empty, skip it.
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) return false;

            if (ItsABuiltInCommand(line)) // If it's a built-in command.
            {
                BuiltInCommand command = GetBuiltItCommand(line);

                // If the command is Else.
                if (command == BuiltInCommand.ELSE)
                {
                    if (Init.ifBlocks.Count > 0) { Init.inAnElseStatement = true; }
                    else { ExceptionsManager.EndBlockDetectedBeforeDefiningANewOne("If", "Else"); }
                }
                // If the command is ElseIf.
                else if (command == BuiltInCommand.ELSEIF)
                {
                    if (Init.ifBlocks.Count > 0)
                    {
                        var elseIfParameters = GetBuiltInCommandParameters(line);
                        return ExecuteCommand(command, elseIfParameters, out result);
                    }
                }
                // If the commmand is EndIf, remove the last If code block.
                else if (command == BuiltInCommand.ENDIF)
                {
                    if (Init.ifBlocks.Count > 0) Init.ifBlocks.Pop();
                    Init.inAnElseStatement = false;
                }

                // First check if it's inside of a If block, if that's the case, check if this code should be executed.
                if (Init.ifBlocks.Count > 0)
                {
                    // Just execute if the "If" is false and it's an else code block
                    // OR if the "If" is true and it's NOT in an else code block.
                    if ((!Init.ifBlocks.Peek() && !Init.inAnElseStatement) || (Init.ifBlocks.Peek() && Init.inAnElseStatement))
                    {
                        return false;
                    }
                }

                #region Just to skip the functions keywords
                if (command == BuiltInCommand.FUNC)
                {
                    Init.insideOfAFunctionBlock = true;
                    return false;
                }
                if (command == BuiltInCommand.ENDFUNC)
                {
                    Init.insideOfAFunctionBlock = false;
                    return false;
                }
                if (Init.insideOfAFunctionBlock) return false;
                #endregion

                var parameters = GetBuiltInCommandParameters(line);
                return ExecuteCommand(command, parameters, out result);
            }
            else // It's a function with PARENTHESIS.
            {
                // First check if it's inside of a If block, if that's the case, check if this code should be executed.
                if (Init.ifBlocks.Count > 0)
                {
                    // Just execute if the "If" is false and it's an else code block
                    // OR if the "If" is true and it's NOT in an else code block.
                    if ((!Init.ifBlocks.Peek() && !Init.inAnElseStatement) || (Init.ifBlocks.Peek() && Init.inAnElseStatement))
                    {
                        return false;
                    }
                }

                // If it's inside of a function code block and this time can't execute it, return.
                if (Init.insideOfAFunctionBlock && !executeFunc) return false;

                // First of all, check if the command is an assigment one.
                // The method itself manages the variable assigment, just return false.
                if (TryToAssign(line)) return false;

                // Just get the function name and parameters and execute it.
                string command = GetFunction(line);
                if (string.IsNullOrEmpty(command)) return false;
                var parameters = GetFunctionParameters(line);
                return ExecuteFunction(command, parameters, out result);
            }
        }

        // To check if the specified command it's a buit-in one.
        public static bool ItsABuiltInCommand(string commandLine)
        {
            // Split the command in spaces.
            string[] splited = commandLine.SplitWithSpaces();

            // If the first text can be parsed to a Built-In command, that means it's a built-in one.
            return Enum.TryParse(typeof(BuiltInCommand), splited[0].Trim(), true, out object? result);
        }

        #region Built-In Commands
        // To check if the given commands are built-in and throw no errors, first use the "ItsABuiltInCommand" method.

        // Gets the built-in command.
        public static BuiltInCommand GetBuiltItCommand(string commandLine)
        {
            // Splits the command into spaces.
            string[] splited = commandLine.SplitWithSpaces();

            // Try to parse the first element (the command itself) to the enum.
            if (Enum.TryParse(typeof(BuiltInCommand), splited[0].Trim(), true, out object? result))
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
            object[] splitedIntoSpaces = commandLine.SplitWithSpaces().Skip(1).ToArray();

            // Combine all this into a simple string.
            string combinedText = string.Join("", splitedIntoSpaces);

            // If the "combinedText" string is empty, there are NO parameters.
            if (string.IsNullOrEmpty(combinedText)) return Array.Empty<object>();

            // Finally, split again by commas.
            object[] result = combinedText.Split(",");

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
            // FUNC, ENDFUNC, ELSE and ENDIF aren´t here because they are managed in the ExecuteLine method.
            switch (commandType)
            {
                case BuiltInCommand.IMPORT:
                    result = null;
                    return BuiltInCommands.Import(commandType, parameters);

                case BuiltInCommand.VAR:
                    result = null;
                    return BuiltInCommands.Var(commandType, parameters);

                case BuiltInCommand.RETURN:
                    return BuiltInCommands.Return(commandType, parameters, out result);

                case BuiltInCommand.IF:
                    result = null;
                    return BuiltInCommands.If(commandType, parameters);

                case BuiltInCommand.ELSEIF:
                    result = null;
                    return BuiltInCommands.ElseIf(commandType, parameters);
            }

            result = null;
            return false;
        }
        #endregion

        #region Functions
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
            string command = commandLine.Substring(0, parenthesisIndex).Trim();
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

        // Executes the specified function.
        public static bool ExecuteFunction(string function, object[] parameters)
        {
            return ExecuteFunction(function, parameters, out object? result);
        }
        public static bool ExecuteFunction(string function, object[] parameters, out object? result, bool skipErrors = false)
        {
            result = null;

            // First check if the function is a custom one, if that's the case, this should be true.
            if (Init.customFunctions.Any(func => func.name == function && func.parameters.Count == parameters.Length))
            {
                // Get the function.
                CustomFunction func = Init.customFunctions.Find(func => func.name == function);

                // First iterate foreach parameter and add them add them a new variable.
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    Init.variables.Add(func.parameters[i], parameters[i]);
                }

                // Foreach line inside of the function block, execute the commands or functions.
                int currentLineBeforeExecution = Init.currentLine;
                for (int i = func.startIndex + 1; i < Init.fileLines.Length; i++)
                {
                    Init.currentLine = i + 1;
                    if (Init.fileLines[i] == "EndFunc") break;
                    if (ExecuteLine(Init.fileLines[i].Trim(), out object? customFuncResult, true))
                    {
                        if (ItsABuiltInCommand(Init.fileLines[i].Trim()))
                        {
                            if (GetBuiltItCommand(Init.fileLines[i].Trim()) == BuiltInCommand.RETURN)
                            {
                                result = customFuncResult;
                            }
                        }
                    }
                }

                Init.currentLine = currentLineBeforeExecution;

                // Delete the function's variables.
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    Init.variables.Remove(func.parameters[i]);
                }

                // The function was executed successfully.
                return true;
            }

            // If not, check in the built-in libraries that are LOADED.
            foreach (Library library in Init.loadedLibraries)
            {
                // If there's a library that contains the specified function WITH THE FULL NAME.
                if (library.avaiableFunctions.Contains(function))
                {
                    return library.ExecuteFunction(function, parameters, out result); // Execute it.
                }
                // If there's a library that contains the specified function WITH THE SHORT NAME.
                else if (library.avaiableFunctions.Any(str => str.Substring(str.IndexOf('.') + 1) == function))
                {
                    return library.ExecuteFunction(function, parameters, out result); // Execute it.
                }
            }

            // If nothing works, throw an error and return false.
            if (!skipErrors) ExceptionsManager.FunctionNotFound(function);

            result = null;
            return false;
        }

        public static void CreateCustomFunction(string line, int currentLine)
        {
            // If the line is null of empty, skip it.
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) return;

            // If it's a built-in command.
            if (ItsABuiltInCommand(line))
            {
                // Get the Built-In command.
                BuiltInCommand command = GetBuiltItCommand(line);

                if (command == BuiltInCommand.FUNC) // This means the command is the start of a custom function.
                {
                    if (Init.ifBlocks.Count > 0) // You can't write functions inside of an if block.
                    {
                        ExceptionsManager.CantDefineFunctionsInsideOfBlock("If");
                        return;
                    }
                    if (Init.insideOfAFunctionBlock) // To begin a new function you need to close the last one first, lol.
                    {
                        ExceptionsManager.FunctionDetectedBeforeClosingTheLastOne();
                        return;
                    }

                    Init.insideOfAFunctionBlock = true;

                    // Extract all the function info.
                    string funcNameWithParenthesis = GetBuiltInCommandParameters(line)[0].ToString();
                    funcName = GetFunction(funcNameWithParenthesis);
                    if (!Utilities.ValidFunctionName(funcName)) // If the function name isn't a valid one, throw an error.
                    {
                        ExceptionsManager.InvalidFunctionName(funcName);
                        funcName = "";
                        Init.insideOfAFunctionBlock = false;
                        return;
                    }
                    // Also you can't create a new function if it's name has been already used by another one or a variable.
                    if (Init.variables.ContainsKey(funcName) || Init.customFunctions.Any(func => func.name == funcName))
                    {
                        ExceptionsManager.VariableOrFunctionAlreadyDefined(funcName);
                        funcName = "";
                        Init.insideOfAFunctionBlock = false;
                        return;
                    }

                    // Finally, if everything it's fine, create it.
                    var parameters = GetFunctionParameters(funcNameWithParenthesis, true);
                    foreach (var parm in parameters) { funcParameters.Add(parm.ToString()); }
                    funcStartIndex = currentLine;
                }
                if (command == BuiltInCommand.RETURN)
                {
                    funcReturnsSomething = true; // If the function has a return statement, that means the functions return something.
                }
                if (command == BuiltInCommand.ENDFUNC)
                {
                    // If this is false that means there is a end block before even starting a new function, that's makes no sense, just do nothing.
                    if (!Init.insideOfAFunctionBlock)
                    {
                        return;
                    }
                    // The functions ends here, add the function to the custom functions list with all the extracted info.
                    Init.insideOfAFunctionBlock = false;
                    Init.customFunctions.Add(new CustomFunction(funcName, funcParameters, funcStartIndex, funcReturnsSomething));
                    funcName = "";
                    funcParameters = new List<string>();
                    funcStartIndex = 0;
                }
                if (command == BuiltInCommand.IF)
                {
                    Init.ifBlocks.Push(false);
                }
                if (command == BuiltInCommand.ENDIF)
                {
                    if (Init.ifBlocks.Count > 0)
                    {
                        Init.ifBlocks.Pop();
                    }
                    else
                    {
                        ExceptionsManager.EndBlockDetectedBeforeDefiningANewOne("If", "EndBlock");
                        return;
                    }
                }
            }
        }
        #endregion

        #region Main Utilities
        // Try to convert the specified text into a value.
        public static dynamic GetValue(string text)
        {
            // Check if the value is null or undefined.
            if (text == "null" || text == "Null" || text == "undefined" || text == "Undefined")
            {
                return null;
            }

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

            // Check if there's a variable with that name.
            if (Init.variables.ContainsKey(text))
            {
                return Init.variables[text];
            }

            // Check if the value can be treated as a IF statement.
            if (EvaluateIfStatement(text, out bool ifResult))
            {
                return ifResult;
            }

            // Check if the value can be parsed to a bool operation.
            if (BoolOperator(text, out bool boolResult))
            {
                return boolResult;
            }

            // Try to check if it's an aritmetical operation.
            object aritmeticOperationResult = AritmeticOperationOrConcatenation(text);
            if (aritmeticOperationResult != null) return aritmeticOperationResult;

            // If by any chance it's a function itself.
            string funcName = GetFunction(text, true);
            object[] funcParameters = GetFunctionParameters(text);
            if (ExecuteFunction(funcName, funcParameters, out object? result, true))
            {
                return result;
            }

            // If nothing works, just return the same text WITHOUT modifications.
            return text;
        }
        // Tries to resolve the specified text as a aritmetical operation.
        public static object AritmeticOperationOrConcatenation(string text)
        {
            #region Should we execute this thing?
            // Check if should execute or not.
            bool execute = false;
            foreach (char c in text)
            {
                // If the string doesn't contain an operator, do nothing, return.
                if (Utilities.IsOperator(c)) execute = true;
            }
            if (!execute) return null;
            #endregion

            #region Tokenize
            // Create the "tokens"
            text = text.RemoveWhitespaces();
            List<string> tokens = new List<string>();
            string currentToken = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (Utilities.IsOperator(c) || c == '(' || c == ')')
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
            #endregion

            #region Solve Parenthesis First
            // If there are any parenthesis inside, solve them first.
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "(")
                {
                    for (int j = tokens.Count - 1; j >= 0; j--)
                    {
                        if (tokens[j] == ")")
                        {
                            string toPass = string.Join("", tokens.GetRange(i + 1, j - 4));
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

                if (Utilities.IsOperator(tokens[i].ToCharArray()[0]))
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
        #endregion

        #region Bool Operators
        // Tries to convert a command into a boolean expression.
        public static bool BoolOperator(string commandLine, out bool result)
        {
            result = false;
            try
            {
                if (commandLine.Contains("AND") || commandLine.Contains("OR")) return false;

                int operatorIndex = commandLine.IndexOfAny(new char[] { '=', '!', '<', '>' });

                string @operator = GetOperator(commandLine, operatorIndex);

                string leftExpression = commandLine.Substring(0, operatorIndex).Trim();
                string rightExpression = commandLine.Substring(operatorIndex + @operator.Length).Trim();

                result = Evaluate(leftExpression, @operator, rightExpression);
                return true;
            }
            catch
            {
                result = false;
                return false;
            }
        }
        static string GetOperator(string text, int operatorIndex)
        {
            if (operatorIndex != -1)
            {
                if (text[operatorIndex] == '=' || text[operatorIndex] == '!')
                {
                    if (text[operatorIndex + 1] == '=')
                        return text.Substring(operatorIndex, 2); // "==" or "!="
                }
                else if (text[operatorIndex] == '<' || text[operatorIndex] == '>')
                {
                    if (operatorIndex + 1 < text.Length && text[operatorIndex + 1] == '=')
                        return text.Substring(operatorIndex, 2); // "<=" or ">="
                    else
                        return text.Substring(operatorIndex, 1); // "<" or ">"
                }
            }
            return "";
        }
        static bool Evaluate(string leftExpression, string @operator, string rightOperator)
        {
            switch (@operator)
            {
                case "==":
                    try
                    {
                        return GetValue(leftExpression) == GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation("==", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
                case "!=":
                    try
                    {
                        return GetValue(leftExpression) != GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation("!=", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
                case "<":
                    try
                    {
                        return GetValue(leftExpression) < GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation("<", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
                case ">":
                    try
                    {
                        return GetValue(leftExpression) > GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation(">", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
                case "<=":
                    try
                    {
                        return GetValue(leftExpression) <= GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation("<=", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
                case ">=":
                    try
                    {
                        return GetValue(leftExpression) >= GetValue(rightOperator);
                    }
                    catch
                    {
                        ExceptionsManager.InvalidOperation(">=", GetValue(leftExpression).GetType().Name, GetValue(rightOperator).GetType().Name);
                        return false;
                    }
            }
            return false;
        }
        #endregion

        #region Actions
        // Tries the specified command as an assigment one. 
        public static bool TryToAssign(string commandLine)
        {
            // Remove all the whitespaces in the command.
            commandLine = commandLine.RemoveWhitespaces();

            // The assigment commands ALWAYS have an equals character, ONLY ONE and NO OTHER OPERATORS.
            if (commandLine.Count(ch => ch == '=') != 1 || commandLine.Count(ch => ch == '<') > 0 || commandLine.Count(ch => ch == '<') > 0 ||
                commandLine.Count(ch => ch == '!') > 0) return false;
            string[] splited = commandLine.Split('=');

            // Get the variable name and the new value.
            string variableName = splited[0];
            string newValue = splited[1];

            // This only will work if the specified variable already exists and it's defined.
            if (Init.variables.ContainsKey(variableName))
            {
                Init.variables[variableName] = GetValue(newValue);
                return true;
            }
            else // Else, throw an error.
            {
                ExceptionsManager.UndefinedVariable(variableName);
                return false;
            }
        }
        public static bool EvaluateIfStatement(string text, out bool result)
        {
            result = false;
            string pattern = @"(AND|OR)";

            var splited = Regex.Split(text, pattern);

            var tokens = new List<string>();
            foreach (var item in splited)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    tokens.Add(item);
                }
            }

            if (!tokens.Contains("AND") && !tokens.Contains("OR")) return false;

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
                            object parenthesisResult = AritmeticOperationOrConcatenation(toPass);
                            tokens.RemoveRange(i, j - i + 1);
#pragma warning disable CS8604
                            tokens.Insert(i, parenthesisResult.ToString());
                            break;
                        }
                    }
                }
            }

            // Solve all the operations inside of the final tokens.
            dynamic? finalResult = null;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (string.IsNullOrEmpty(tokens[i])) continue;

                if (tokens[i] == "AND" || tokens[i] == "OR")
                {
                    if (tokens[i] == "AND")
                    {
                        try { finalResult = finalResult && GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return false; }
                    }
                    if (tokens[i] == "OR")
                    {
                        try { finalResult = finalResult || GetValue(tokens[i + 1]); }
                        catch { ExceptionsManager.InvalidOperation(tokens[i], finalResult.GetType().Name, tokens[i + 1].GetType().Name); return false; }
                    }
                    i++;
                }
                else
                {
                    if (finalResult == null) finalResult = GetValue(tokens[i]);
                }
            }

            result = finalResult;
            return true;
        }
        #endregion

        public static void AfterFirstReadCheck()
        {
            // If the If Blocks count is greater than 0, that means an If block wasn't closed before reaching the end of the file.
            if (Init.ifBlocks.Count > 0)
            {
                ExceptionsManager.BlockNotClosed("If");
            }
            // If true that means no EndFunc code was reached. Throw an error.
            if (Init.insideOfAFunctionBlock)
            {
                ExceptionsManager.FunctionsWasntClosed(funcName);
                //customFunctions.Remove(customFunctions.Last());
            }

            // Resset this.
            Init.currentLine = 0;
            Init.insideOfAFunctionBlock = false;
            Init.ifBlocks = new Stack<bool>();
        }
    }
}
