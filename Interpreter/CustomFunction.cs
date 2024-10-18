using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class CustomFunction
    {
        public string name;
        public List<string> parameters;
        public int startIndex;
        public bool returnsAValue;

        public CustomFunction(string name, List<string> parameters, int startIndex, bool returnsAValue)
        {
            this.name = name;
            this.parameters = parameters;
            this.startIndex = startIndex;
            this.returnsAValue = returnsAValue;
        }
    }
}
