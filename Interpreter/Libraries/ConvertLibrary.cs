using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Libraries
{
    public class ConvertLibrary : Library
    {
        public ConvertLibrary()
        {
            avaiableFunctions = new List<string>()
            {
                "convert.int",
                "convert.str",
                "convert.float"
            };
        }

        public override bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            switch (command)
            {
                case "convert.int":
                case "int":
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    result = ConvertToInt(parameters[0]);
                    return true;

                case "convert.str":
                case "str":
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    result = ConvertToString(parameters[0]);
                    return true;

                case "convert.float":
                case "float":
                    if (parameters.Length != 1)
                    {
                        ExceptionsManager.IncorrectFunctionParametersNumber(command, 1);
                        break;
                    }
                    result = ConvertToFloat(parameters[0]);
                    return true;
            }

            result = null;
            return false;
        }

        int? ConvertToInt(object value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                ExceptionsManager.CantConvertFromTo(value, value.GetType().Name, "Int");
                return null;
            }
        }

        string? ConvertToString(object value)
        {
            try
            {
                return Convert.ToString(value);
            }
            catch
            {
                ExceptionsManager.CantConvertFromTo(value, value.GetType().Name, "String");
                return null;
            }
        }

        float? ConvertToFloat(object value)
        {
            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                ExceptionsManager.CantConvertFromTo(value, value.GetType().Name, "Float");
                return null;
            }
        }
    }
}
