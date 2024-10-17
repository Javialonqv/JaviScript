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
        public static void IncorrectFunctionParametersNumber(string functionName, int parametersCount)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function doesn't take {parametersCount} parameters.");
        }
        public static void FunctionNotFound(string functionName)
        {
            PrintError(Program.currentLine, $"The \"{functionName}\" function can´t be found. Are you missing a library?");
        }
        public static void UndefinedVariable(string variableNmae)
        {

        }
    }
}
