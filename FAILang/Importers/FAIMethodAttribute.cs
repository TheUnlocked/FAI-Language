using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Importers
{
    public class FAIMethodAttribute : Attribute
    {
        public string FunctionName { get; }
        public string[] ArgList { get; }
        public bool Memoize { get; }
        public bool VariableArgs { get; }

        public FAIMethodAttribute(string name, string[] argList, bool memoize = false, bool variableArgs = false)
        {
            FunctionName = name;
            ArgList = argList;
            Memoize = memoize;
            VariableArgs = variableArgs;
        }
    }
}
