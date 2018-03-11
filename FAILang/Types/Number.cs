using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace FAILang.Types
{
    public struct Number : IOperable
    {
        public string TypeName => "Number";

        public readonly Complex value;
        public bool IsReal => value.Imaginary == 0;

        public Dictionary<Operator, Func<IOperable, IType>> Operators => new Dictionary<Operator, Func<IOperable, IType>>() {
            {Operator.ADD, Add},
            {Operator.SUBTRACT, Subtract},
            {Operator.MULTIPLY, Multiply},
            {Operator.DIVIDE, Divide},
            {Operator.MODULO, Modulo},
            {Operator.EXPONENT, Exponent},

            {Operator.GREATER, GreaterThan},
            {Operator.LESS, LessThan},
            {Operator.GR_EQUAL, GreaterEqual},
            {Operator.LE_EQUAL, LessEqual}
        };

        public Number(Complex value)
        {
            this.value = value;
        }

        public IType Add(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value + num.value);
                default:
                    return null;
            }
        }
        public IType Subtract(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value - num.value);
                default:
                    return null;
            }
        }
        public IType Multiply(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value * num.value);
                default:
                    return null;
            }
        }
        public IType Divide(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (num.IsReal && num.value.Real == 0)
                        return Void.instance;
                    return new Number(value / num.value);
                default:
                    return null;
            }
        }
        public IType Modulo(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (num.value.Real == 0 || !num.IsReal)
                        return Void.instance;
                    return new Number(new Complex(((value.Real % num.value.Real) + num.value.Real) % num.value.Real,
                                             ((value.Imaginary % num.value.Real) + num.value.Real) % num.value.Real));
                default:
                    return null;
            }
        }
        public IType Exponent(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value / num.value);
                default:
                    return null;
            }
        }


        public override string ToString()
        {
            if (value.Imaginary != 0 && !double.IsNaN(value.Real))
            {
                if (value.Real != 0)
                {
                    if (value.Imaginary == 1)
                        return $"{value.Real}+i";
                    else if (value.Imaginary == -1)
                        return $"{value.Real}-i";
                    else if (value.Imaginary > 0 && value.Imaginary != 1)
                        return $"{value.Real}+{value.Imaginary}i";
                    else
                        return $"{value.Real}{value.Imaginary}i";
                }
                else if (value.Imaginary == 1)
                    return $"i";
                else if (value.Imaginary == -1)
                    return $"-i";
                else
                    return $"{value.Imaginary}i";
            }
            return value.Real.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Number num)
                return value.Equals(num.value);
            return false;
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<Complex>.Default.GetHashCode(value);
        }
    }
}
