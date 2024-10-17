using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Libraries
{
    public class Library
    {
        public List<string> avaiableFunctions { get; set; } = new List<string>(); 

        public virtual bool ExecuteFunction(string command, object[] parameters, out object? result)
        {
            result = null;
            return false;
        }
    }
}
