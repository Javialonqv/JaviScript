using System;
using System.Windows.Forms;
using Interpreter.Libraries;

namespace Interpreter
{
    internal class Init
    {
        string codeFilePath = "";

        public static string[] fileLines = Array.Empty<string>();
        public static int currentLine;
        public static int realCurrentLine;

        public static List<Library> loadedLibraries = new List<Library>();
        public static List<CustomFunction> customFunctions = new List<CustomFunction>();
        public static List<Variable> variables = new List<Variable>();
        public static List<Variable> inFuncVariables = new List<Variable>();
        public static Stack<bool> ifBlocks = new Stack<bool>();
        public static Stack<(bool result, int line, string command)> whileBlocks = new Stack<(bool result, int line, string command)>();

        public static bool insideOfAFunctionBlock = false;
        public static bool inAnElseStatement;

        [STAThread]
        static void Main(string[] args)
        {
            Init program = new Init();
            loadedLibraries.Add(new MainLibrary());
            program.RequestCodeFile();
            program.ReadCodeFile();
            
            if (program.FirstRead())
            {
                program.ExecuteLines();
            }

            try
            {
                //Init program = new Init();
                //loadedLibraries.Add(new MainLibrary());
                //program.RequestCodeFile();
                //program.ReadCodeFile();
                //program.FirstRead();
                //program.ExecuteLines();
            }
            catch (Exception e)
            {
                ExceptionsManager.CriticalError(e.Message);
            }
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

        bool FirstRead() // First read to the file to detect the custom functions.
        {
            for (int i = 0; i < fileLines.Length; i++) // Iterate foreach line in the code file.
            {
                currentLine = i + 1;
                string line = fileLines[i];

                if (!Interpreter.CreateCustomFunction(line, i)) // If the interpreter returns false, an error was thrown.
                {
                    return false;
                }
            }

            return Interpreter.AfterFirstReadCheck();
        }

        void ExecuteLines()
        {
            // Iterate foreach file line and execute it's code.
            for (realCurrentLine = 0; realCurrentLine < fileLines.Length; realCurrentLine++)
            {
                currentLine = realCurrentLine + 1;
                string line = fileLines[realCurrentLine];

                Interpreter.ExecuteLine(line);
            }
        }
    }
}