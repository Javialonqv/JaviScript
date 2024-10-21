using System;

namespace Interpreter
{
    public class Variable
    {
        public string name;
        public object value;
        public static List<string> avaiableFunctions = new()
        {
            "getType"
        };

        public Variable(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public static bool ExecuteFunction(string command, object[] parameters, Variable instance, out object? result)
        {
            switch (command)
            {
                case "getType":
                    if (parameters.Length != 0)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, parameters.Length, "0");
                        break;
                    }
                    result = instance.value.GetType().Name;
                    return true;
            }

            result = null;
            return false;
        }
    }
}