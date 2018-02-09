using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    class MathBool : IType
    {
        public static readonly MathBool TRUE = new MathBool(true);
        public static readonly MathBool FALSE = new MathBool(false);

        public string TypeName => "Boolean";
        public bool value;

        private MathBool(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value ? "true" : "false";
        }
    }
}
