using FAILang.Types;
using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FAILang
{
    public enum Operator
    {
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MODULO,
        EXPONENT,
        EQUALS,
        NOT_EQUALS,
        GREATER,
        LESS,
        GR_EQUAL,
        LE_EQUAL
    }

    public static class OperatorMethods
    {
        public static IType Operate(this Operator op, IType left, IType right)
        {
            if (left is IPopup)
                if (!(left is Types.Void && (op == Operator.EQUALS || op == Operator.NOT_EQUALS)))
                    return left;
            if (right is IPopup)
                if (!(right is Types.Void && (op == Operator.EQUALS || op == Operator.NOT_EQUALS)))
                    return right;

            Number n1 = left as Number;
            Number n2 = right as Number;

            Types.Vector v1 = left as Types.Vector;
            Types.Vector v2 = right as Types.Vector;

            MathString s1 = left as MathString;
            MathString s2 = right as MathString;

            MathBool b1 = left as MathBool;
            MathBool b2 = right as MathBool;

            switch (op)
            {
                case Operator.ADD:
                    if (n1 != null && n2 != null)
                        return new Number(n1.value + n2.value);
                    if (v1 != null && v2 != null)
                        return new Types.Vector(v1.items.Combine(v2.items, (x, y) => Operator.ADD.Operate(x, y)));
                    else if (s1 != null && s2 != null)
                        return new MathString(s1.value + s2.value);
                    else if (b1 != null && b2 != null)
                        return b1.value || b2.value ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The + operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.SUBTRACT:
                    if (n1 != null && n2 != null)
                        return new Number(n1.value - n2.value);
                    return new Error("WrongType", $"The - operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.MULTIPLY:
                    if (n1 != null && n2 != null)
                        return new Number(n1.value * n2.value);
                    else if (v1 != null && v2 != null)
                    {
                        return v1.items.Combine(v2.items, (x, y) => Operator.MULTIPLY.Operate(x, y)).Aggregate((x, y) => Operator.ADD.Operate(x, y));
                    }
                    else if ((v1 ?? v2) != null && (n1 ?? n2) != null)
                    {
                        return new Types.Vector((v1 ?? v2).items.Select((x, y) => Operator.MULTIPLY.Operate(x, n1 ?? n2)).ToArray());
                    }
                    else if (b1 != null && b2 != null)
                        return b1.value && b2.value ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The * operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.DIVIDE:
                    if (n1 != null && n2 != null)
                        return new Number(n1.value / n2.value);
                    else if ((v1 ?? v2) != null && (n1 ?? n2) != null)
                    {
                        return new Types.Vector((v1 ?? v2).items.Select((x, y) => Operator.DIVIDE.Operate(x, n1 ?? n2)).ToArray());
                    }
                    return new Error("WrongType", $"The / operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.MODULO:
                    if (n1 != null && n2 != null)
                    {
                        double mod(double a, double n) => n == 0 ? 0 : ((a % n) + n) % n;
                        return new Number(new Complex(mod(n1.value.Real, n2.value.Real),
                                                    mod(n1.value.Imaginary, n2.value.Imaginary)));
                    }
                    return new Error("WrongType", $"The / operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.EXPONENT:
                    if (n1 != null && n2 != null)
                    {
                        if (n1.IsReal && n2.IsReal)
                        {
                            return new Number(Math.Pow(n1.value.Real, n2.value.Real));
                        }
                        return new Number(Complex.Pow(n1.value, n2.value));
                    }
                    return new Error("WrongType", $"The / operator cannot be applied to types {left.TypeName} and {right.TypeName}");

                case Operator.EQUALS:
                    return left.Equals(right) ? MathBool.TRUE : MathBool.FALSE;
                case Operator.NOT_EQUALS:
                    return left.Equals(right) ? MathBool.FALSE : MathBool.TRUE;
                case Operator.GREATER:
                    if (n1 != null && n2 != null)
                        if (n1.value.Real != n2.value.Real)
                            return (n1.value.Real > n2.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                        else
                            return (n1.value.Imaginary > n2.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The > comparison operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.LESS:
                    if (n1 != null && n2 != null)
                        if (n1.value.Real != n2.value.Real)
                            return (n1.value.Real < n2.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                        else
                            return (n1.value.Imaginary < n2.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The < comparison operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.GR_EQUAL:
                    if (n1 != null && n2 != null)
                        if (n1.value.Real != n2.value.Real)
                            return (n1.value.Real >= n2.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                        else
                            return (n1.value.Imaginary >= n2.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The >= comparison operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                case Operator.LE_EQUAL:
                    if (n1 != null && n2 != null)
                        if (n1.value.Real != n2.value.Real)
                            return (n1.value.Real <= n2.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                        else
                            return (n1.value.Imaginary <= n2.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                    return new Error("WrongType", $"The <= comparison operator cannot be applied to types {left.TypeName} and {right.TypeName}");
            }
            return new Error("UnexpectedError", "An unexpected error occured with an operator.");
        }

        private static T[] Combine<T>(this T[] l1, T[] l2, Func<T, T, T> f) where T : IType {
            if (l1.Length == l2.Length)
            {
                T[] newL = new T[l1.Length];
                for (int i = 0; i < l1.Length; i++)
                    newL[i] = f.Invoke(l1[i], l2[i]);
                return newL;
            }
            return null;
        }
    }
}
