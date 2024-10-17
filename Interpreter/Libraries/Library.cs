using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Libraries
{
    public abstract class Library
    {
        public List<string> avaiableFunctions;

        public abstract bool ExecuteFunction(string command, object[] parameters, out object? result);
    }
}
