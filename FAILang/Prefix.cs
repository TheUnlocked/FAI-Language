using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang
{
    public enum Prefix
    {
        NOT,
        NEGATIVE
    }

    public static class PrefixMethods
    {
        public static IType Operate(this Prefix op, IType target)
        {
            if (target is IPopup)
                return target;
            switch (op)
            {
                case Prefix.NOT:
                    if (target is MathBool b)
                    {
                        return b.value ? MathBool.FALSE : MathBool.TRUE;
                    }
                    return new Error("WrongType", $"The ~ prefix cannot be applied to type {target.TypeName}");
                case Prefix.NEGATIVE:
                    if (target is Number n)
                    {
                        return new Number(-n.value);
                    }
                    return new Error("WrongType", $"The - prefix cannot be applied to type {target.TypeName}");
            }
            return new Error("UnexpectedError", "An unexpected error occured with a prefix.");
        }
    }
}
