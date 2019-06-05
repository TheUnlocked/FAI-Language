using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public class Undefined : IType
    {
        public string TypeName => "Undefined";
        public static readonly Undefined instance = new Undefined();
        public static Undefined Instance => instance;

        public override string ToString()
        {
            return "undefined";
        }
    }
}
