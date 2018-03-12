using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    class MathBool : IOperable
    {
        public static readonly MathBool TRUE = new MathBool(true);
        public static readonly MathBool FALSE = new MathBool(false);

        public string TypeName => "Boolean";
        public bool value;

        private MathBool(bool value)
        {
            this.value = value;
        }

        public Dictionary<Operator, Func<IOperable, IType>> Operators => new Dictionary<Operator, Func<IOperable, IType>>() {
            {Operator.ADD, OpOr},
            {Operator.MULTIPLY, OpAnd},
            {Operator.EXPONENT, OpXor}
        };

        private IType OpOr(IOperable other)
        {
            switch (other)
            {
                case MathBool b:
                    return b.value || value ? TRUE : FALSE;
                default:
                    return null;
            }
        }
        private IType OpAnd(IOperable other)
        {
            switch (other)
            {
                case MathBool b:
                    return b.value && value ? TRUE : FALSE;
                default:
                    return null;
            }
        }
        private IType OpXor(IOperable other)
        {
            switch (other)
            {
                case MathBool b:
                    return b.value ^ value ? TRUE : FALSE;
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return value ? "true" : "false";
        }
    }
}
