using System;
using System.Windows.Forms;
using Interpreter.Libraries;

namespace Interpreter
{
    internal class Program
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string codeFilePath;

        string[] fileLines;
        public static int currentLine;

        public static List<Library> loadedLibraries = new List<Library>();
        public static Dictionary<string, object> variables = new Dictionary<string, object>();

        [STAThread]
        static void Main(string[] args)
        {
            //Console.WriteLine(new ExpressionEvaluator().Evaluate("22 + 22"));
            //return;
            Program program = new Program();
            loadedLibraries.Add(new MainLibrary());
            program.RequestCodeFile();
            program.ReadCodeFile();
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

        void ExecuteCommands()
        {
            for (int i = 0; i < fileLines.Length; i++)
            {
                currentLine = i + 1;
                string line = fileLines[i];

                if (string.IsNullOrEmpty(line)) continue;

                if (Interpreter.ItsABuiltInCommand(line))
                {
                    BuiltInCommand command = Interpreter.GetBuiltItCommand(line);
                    var parameters = Interpreter.GetBuiltInCommandParameters(line);
                    Interpreter.ExecuteCommand(command, parameters);
                }
                else
                {
                    string command = Interpreter.GetFunction(line);
                    if (string.IsNullOrEmpty(command)) continue;
                    var parameters = Interpreter.GetFunctionParameters(line);
                    Interpreter.ExecuteFunction(command, parameters);
                }
            }
        }
    }
}