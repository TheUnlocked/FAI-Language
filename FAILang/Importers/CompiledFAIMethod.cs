using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Importers
{
    class CompiledFAIMethod : Attribute
    {
        public string FunctionName { get; }
        public string[] ArgList { get; }

        public CompiledFAIMethod(string name, string[] argList)
        {
            FunctionName = name;
            ArgList = argList;
        }
    }
}
