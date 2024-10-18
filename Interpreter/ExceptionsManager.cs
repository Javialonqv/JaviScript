using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class ExceptionsManager
    {
        public static void PrintError(int line, string errorMessage)
        {
            string text = $"Line {line}: {errorMessage}";
            MessageBox.Show(text, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void BuiltInCommandNotFound(string commandName)
        {
            PrintError(Init.currentLine, $"The built-in command \"{commandName}\" can´t be found.");
        }
        public static void IncorrectCommandParametersNumber(string commandName, int parametersCount)
        {
            PrintError(Init.currentLine, $"The built-in command \"{commandName}\" doesn't take {parametersCount} parameters.");
        }

        public static void NoFunctionParenthesisFound(string functionName)
        {
            PrintError(Init.currentLine, $"The \"{functionName}\" function's parenthesis can't be found.");
        }
        public static void FunctionNotFound(string functionName)
        {
            PrintError(Init.currentLine, $"The \"{functionName}\" function can´t be found. Are you missing a library?");
        }
        public static void IncorrectFunctionParametersNumber(string functionName, int parametersCount)
        {
            PrintError(Init.currentLine, $"The \"{functionName}\" function doesn't take {parametersCount} parameters.");
        }
        public static void InvalidFunctionParameterType(string functionName, int parameterIndex, string givenType, string expectedType)
        {
            PrintError(Init.currentLine, $"The \"{parameterIndex + 1}\" parameter in the \"{functionName}\" function doesn't take a {givenType} value. Has to be {expectedType}.");
        }
        public static void FunctionsWasntClosed(string functionName)
        {
            PrintError(Init.currentLine, $"The \"{functionName}\" function wasn't closed using \"EndFunc\".");
        }
        public static void FunctionDetectedBeforeClosingTheLastOne()
        {
            PrintError(Init.currentLine, $"A new function was detected before the last one was closed using \"EndFunc\".");
        }

        public static void CantDefineFunctionsInsideOfBlock(string blockName)
        {
            PrintError(Init.currentLine, $"Can't define a function inside of a {blockName} block.");
        }
        public static void BlockNotClosed(string blockName)
        {
            PrintError(Init.currentLine, $"A {blockName} wasn't closed.");
        }
        public static void EndBlockDetectedBeforeDefiningANewOne(string blockName, string endBlockName)
        {
            PrintError(Init.currentLine, $"{endBlockName} was detected before a new one could be declared using \"{blockName}\".");
        }
        public static void InvalidOperation(string @operator, string firstType, string secondType)
        {
            PrintError(Init.currentLine, $"The \"{@operator}\" operator is not valid with types \"{firstType}\" and \"{secondType}\".");
        }
        public static void InvalidFunctionName(string functionName)
        {
            PrintError(Init.currentLine, $"The funciton name \"{functionName}\" is NOT valid!");
        }
        public static void LibraryNotFound(string libraryName)
        {
            PrintError(Init.currentLine, $"The \"{libraryName}\" can't be found!");
        }
        public static void UndefinedVariable(string variableName)
        {
            PrintError(Init.currentLine, $"The variable of name \"{variableName}\" it's undefined.");
        }
        public static void VariableOrFunctionAlreadyDefined(string name)
        {
            PrintError(Init.currentLine, $"The variable or function \"{name}\" is already defined.");
        }
        public static void CantConvertFromTo(object value, string startType, string endType)
        {
            PrintError(Init.currentLine, $"Can't convert \"{value}\" from type \"{startType}\" to type \"{endType}\".");
        }
    }
}
