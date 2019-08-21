using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public class Undefined : IType
    {
        public string TypeName => "Undefined";
        public static Undefined Instance { get; } = new Undefined();

        public override string ToString()
        {
            return "undefined";
        }
    }
}
