using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public class Void : Error
    {
        public override string TypeName => "Void";
        public static readonly Void instance = new Void();

        private Void() : base("", "")
        {
        }

        public override string ToString()
        {
            return "void";
        }
    }
}
