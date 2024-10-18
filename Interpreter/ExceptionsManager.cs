using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class ExceptionsManager
    {
        static void PrintError(int line, string errorMessage)
        {
            string text = $"Line {line}: {errorMessage}";
            MessageBox.Show(text, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void BuiltInCommandNotFound(string commandName)
        {
            PrintError(Program.currentLine, $"The built-in command \"{commandName}\" can´t be found.");
        }
        public static void IncorrectCommandParametersNumber(string commandName, int parametersCount)
        {
            PrintError(Program.currentLine, $"The built-in command \"{commandName}\" doesn't take {parametersCount} parameters.");
        }

        public static void NoFunctionParenthesisFound(string functionName)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function's parenthesis can't be found.");
        }
        public static void FunctionNotFound(string functionName)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function can´t be found. Are you missing a library?");
        }
        public static void IncorrectFunctionParametersNumber(string functionName, int parametersCount)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function doesn't take {parametersCount} parameters.");
        }
        public static void InvalidFunctionParameterType(string functionName, int parameterIndex, string givenType, string expectedType)
        {
            PrintError(Program.currentLine, $"The \"{parameterIndex + 1}\" parameter in the \"{functionName}\" function doesn't take a {givenType} value. Has to be {expectedType}.");
        }
        public static void FunctionsWasntClosed(string functionName)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function wasn't closed using \"EndFunc\".");
        }
        public static void FunctionDetectedBeforeClosingTheLastOne()
        {
            PrintError(Program.currentLine, $"A new function was detected before the last one was closed using \"EndFunc\".");
        }

        public static void InvalidOperation(string @operator, string firstType, string secondType)
        {
            PrintError(Program.currentLine, $"The \"{@operator}\" operator is not valid with types \"{firstType}\" and \"{secondType}\".");
        }
        public static void LibraryNotFound(string libraryName)
        {
            PrintError(Program.currentLine, $"The \"{libraryName}\" can't be found!");
        }
        public static void UndefinedVariable(string variableName)
        {
            PrintError(Program.currentLine, $"The variable of name \"{variableName}\" it's undefined.");
        }
    }
}
