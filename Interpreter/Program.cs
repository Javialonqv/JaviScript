using System;
using System.Windows.Forms;
using Interpreter.Libraries;

namespace Interpreter
{
    internal class Program
    {
        string codeFilePath = "";

        public static string[] fileLines = Array.Empty<string>();
        public static int currentLine;

        public static List<Library> loadedLibraries = new List<Library>();
        public static List<CustomFunction> customFunctions = new List<CustomFunction>();
        public static Dictionary<string, object> variables = new Dictionary<string, object>();

        [STAThread]
        static void Main(string[] args)
        {
            Program program = new Program();
            loadedLibraries.Add(new MainLibrary());
            program.RequestCodeFile();
            program.ReadCodeFile();
            program.FirstRead();
            program.ExecuteCommands();
        }

        [STAThread]
        // Request code file to the user.
        void RequestCodeFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                codeFilePath = dialog.FileName;
            }
        }
        // Read all the lines of the file.
        void ReadCodeFile()
        {
            if (!File.Exists(codeFilePath)) return;

            fileLines = File.ReadAllLines(codeFilePath);
        }

        // The current function info that is being inspected.
        string funcName = "";
        List<string> funcParameters = new List<string>();
        int funcStartIndex = 0;
        bool funcReturnsSomething = false;
        void FirstRead() // First read to the file to detect the custom functions.
        {
            for (int i = 0; i < fileLines.Length; i++)
            {
                currentLine = i;
                string line = fileLines[i];

                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue; // If the line is null of empty, skip it.

                // If it's a built-in command.
                if (Interpreter.ItsABuiltInCommand(line))
                {
                    BuiltInCommand command = Interpreter.GetBuiltItCommand(line);
                    if (command == BuiltInCommand.FUNC)
                    {
                        if (insideOfAFunctionBlock)
                        {
                            ExceptionsManager.FunctionDetectedBeforeClosingTheLastOne();
                            continue;
                        }

                        insideOfAFunctionBlock = true;

                        // Extract all the function info.
                        string funcNameWithParenthesis = Interpreter.GetBuiltInCommandParameters(line)[0].ToString();
                        funcName = Interpreter.GetFunction(funcNameWithParenthesis);
                        if (!Utilities.ValidFunctionName(funcName)) // If the function name isn't a valid one, throw an error.
                        {
                            ExceptionsManager.InvalidFunctionName(funcName);
                            funcName = "";
                            insideOfAFunctionBlock = false;
                            continue;
                        }

                        var parameters = Interpreter.GetFunctionParameters(funcNameWithParenthesis, true);
                        foreach (var parm in parameters) { funcParameters.Add(parm.ToString()); }
                        funcStartIndex = i;
                    }
                    if (command == BuiltInCommand.RETURN)
                    {
                        funcReturnsSomething = true; // If the function has a return statement, that means the functions return something.
                    }
                    if (command == BuiltInCommand.ENDFUNC)
                    {
                        // If this is false that means there is a end block before even starting a new function, that's makes no sense, just do nothing.
                        if (!insideOfAFunctionBlock)
                        {
                            continue;
                        }
                        // The functions ends here, add the function to the custom functions list with all the extracted info.
                        insideOfAFunctionBlock = false;
                        customFunctions.Add(new CustomFunction(funcName, funcParameters, funcStartIndex, funcReturnsSomething));
                        funcName = "";
                        funcParameters = new List<string>();
                        funcStartIndex = 0;
                    }
                }
            }

            // If true that means no EndFunc code was reached. Throw an error.
            if (insideOfAFunctionBlock)
            {
                ExceptionsManager.FunctionsWasntClosed(customFunctions.Last().name);
                customFunctions.Remove(customFunctions.Last());
            }

            // Resset this.
            currentLine = 0;
            insideOfAFunctionBlock = false;
        }
        void ExecuteCommands()
        {
            // Iterate foreach file line and execute it's code.
            for (int i = 0; i < fileLines.Length; i++)
            {
                currentLine = i + 1;
                string line = fileLines[i];

                ExecuteCommand(line);
            }
        }

        static bool insideOfAFunctionBlock = false;
        public static bool ExecuteCommand(string line,  bool executeFunc = false)
        {
            return ExecuteCommand(line, out object? result, executeFunc);
        }
        public static bool ExecuteCommand(string line, out object? result, bool executeFunc = false)
        {
            result = null;

            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) return false; // If the line is null of empty, skip it.

            // If it's a built-in command.
            if (Interpreter.ItsABuiltInCommand(line))
            {
                BuiltInCommand command = Interpreter.GetBuiltItCommand(line);

                #region Just to skip the functions keywords
                if (command == BuiltInCommand.FUNC)
                {
                    insideOfAFunctionBlock = true;
                    return false;
                }
                if (command == BuiltInCommand.ENDFUNC)
                {
                    insideOfAFunctionBlock = false;
                    return false;
                }
                if (insideOfAFunctionBlock) return false;
                #endregion

                var parameters = Interpreter.GetBuiltInCommandParameters(line);
                return Interpreter.ExecuteCommand(command, parameters, out result);
            }
            else // It's a function with PARENTHESIS.
            {
                if (insideOfAFunctionBlock && !executeFunc) return false; // If it's inside of a function code block and this time can't execute it, return.

                if (Interpreter.TryToAssign(line)) return false;

                // Just get the function name and parameters and execute it.
                string command = Interpreter.GetFunction(line);
                if (string.IsNullOrEmpty(command)) return false;
                var parameters = Interpreter.GetFunctionParameters(line);
                return Interpreter.ExecuteFunction(command, parameters, out result);
            }
        }
    }
}