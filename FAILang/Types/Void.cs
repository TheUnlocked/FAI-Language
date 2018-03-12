using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public class Void : IType
    {
        public string TypeName => "Void";
        public static readonly Void instance = new Void();

        public override string ToString()
        {
            return "void";
        }
    }
}
