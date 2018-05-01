using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    public class MathBool : IOperable
    {
        public static readonly MathBool TRUE = new MathBool(true);
        public static readonly MathBool FALSE = new MathBool(false);

        public string TypeName => "Boolean";
        public bool value;

        private MathBool(bool value)
        {
            this.value = value;
        }

        public Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators => new Dictionary<BinaryOperator, Func<IOperable, IType>>() {
            {BinaryOperator.OR, OpOr},
            {BinaryOperator.AND, OpAnd},
            {BinaryOperator.EXPONENT, OpXor}
        };

        public Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators => null;

        public Dictionary<UnaryOperator, Func<IType>> UnaryOperators => new Dictionary<UnaryOperator, Func<IType>>()
        {
            {UnaryOperator.NOT, OpNot}
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

        private IType OpNot()
        {
            return value ? FALSE : TRUE;
        }

        public override string ToString()
        {
            return value ? "true" : "false";
        }
    }
}
