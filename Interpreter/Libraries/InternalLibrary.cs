using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Interpreter.Libraries
{
    public class InternalLibrary : Library
    {
        public InternalLibrary()
        {
            avaiableFunctions = new List<string>()
            {
                "internal.execute",
                "internal.call",
                "internal.getType"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
                case "internal.execute":
                case "execute":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    Interpreter.ExecuteLine((string)parameters[0]);

                    result = null;
                    return true;

                case "internal.call":
                case "call":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    Call((string)parameters[0]);

                    result = null;
                    return true;

                case "internal.getType":
                case "getType":
                    if (parameters.Length > 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }

                    result = GetType(parameters[0]);
                    return true;
            }

            result = null;
            return false;
        }

        void Call(string script)
        {
            try
            {
                var validReferences = AppDomain.CurrentDomain.GetAssemblies() // Get all the project assemblies.
                    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location)) // Exclude dynamic assemblies.
                    .Select(a => MetadataReference.CreateFromFile(a.Location)); // For each assembly, create a metadata reference.

                var options = ScriptOptions.Default // The default options.
                    .WithReferences(validReferences) // Include all the valid assembly references.
                    .WithImports("System", "System.Linq", "System.Collections.Generic"); // Include main using directives.

                CSharpScript.EvaluateAsync(script, options); // Evaluate and execute.
            }
            catch (Exception e)
            {
                ErrorExecutingCSharpInternalCode(e.Message);
            }
        }
        string GetType(object obj)
        {
            return obj.GetType().Name;
        }

        public void ErrorExecutingCSharpInternalCode(string errorMessage)
        {
            ExceptionsManager.CriticalError($"The internal C# code can't be executed. \"{errorMessage}\".");
        }
    }
}
