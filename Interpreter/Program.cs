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
        void RequestCodeFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                codeFilePath = dialog.FileName;
            }
        }
        void ReadCodeFile()
        {
            fileLines = File.ReadAllLines(codeFilePath);
        }

        string funcName = "";
        List<string> funcParameters = new List<string>();
        int funcStartIndex = 0;
        void FirstRead()
        {
            for (int i = 0; i < fileLines.Length; i++)
            {
                currentLine = i;
                string line = fileLines[i];

                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;

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
                        string funcNameWithParenthesis = Interpreter.GetBuiltInCommandParameters(line)[0].ToString();
                        funcName = Interpreter.GetFunction(funcNameWithParenthesis);
                        var parameters = Interpreter.GetFunctionParameters(funcNameWithParenthesis, true);
                        foreach (var parm in parameters) { funcParameters.Add(parm.ToString()); }
                        funcStartIndex = i;
                    }
                    if (command == BuiltInCommand.ENDFUNC)
                    {
                        insideOfAFunctionBlock = false;
                        customFunctions.Add(new CustomFunction(funcName, funcParameters, funcStartIndex, false));
                        funcName = "";
                        funcParameters = new List<string>();
                        funcStartIndex = 0;
                    }
                }
            }

            if (insideOfAFunctionBlock)
            {
                ExceptionsManager.FunctionsWasntClosed(customFunctions.Last().name);
                customFunctions.Remove(customFunctions.Last());
            }

            currentLine = 0;
            insideOfAFunctionBlock = false;
        }
        void ExecuteCommands()
        {
            for (int i = 0; i < fileLines.Length; i++)
            {
                currentLine = i + 1;
                string line = fileLines[i];

                ExecuteCommand(line);
            }
        }

        static bool insideOfAFunctionBlock = false;
        public static void ExecuteCommand(string line, bool executeFunc = false)
        {
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) return;

            if (Interpreter.ItsABuiltInCommand(line))
            {
                BuiltInCommand command = Interpreter.GetBuiltItCommand(line);
                if (command == BuiltInCommand.FUNC)
                {
                    insideOfAFunctionBlock = true;
                    return;
                }
                if (command == BuiltInCommand.ENDFUNC)
                {
                    insideOfAFunctionBlock = false;
                    return;
                }
                if (insideOfAFunctionBlock) return;

                var parameters = Interpreter.GetBuiltInCommandParameters(line);
                Interpreter.ExecuteCommand(command, parameters);
            }
            else
            {
                if (insideOfAFunctionBlock && !executeFunc) return;

                string command = Interpreter.GetFunction(line);
                if (string.IsNullOrEmpty(command)) return;
                var parameters = Interpreter.GetFunctionParameters(line);
                Interpreter.ExecuteFunction(command, parameters);
            }
        }
    }
}