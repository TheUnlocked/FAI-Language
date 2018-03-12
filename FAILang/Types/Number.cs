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
        
        public Number(Complex value)
        {
            this.value = value;
        }

        public Dictionary<Operator, Func<IOperable, IType>> Operators => new Dictionary<Operator, Func<IOperable, IType>>() {
            {Operator.ADD, OpAdd},
            {Operator.SUBTRACT, OpSubtract},
            {Operator.MULTIPLY, OpMultiply},
            {Operator.DIVIDE, OpDivide},
            {Operator.MODULO, OpModulo},
            {Operator.EXPONENT, OpExponent},

            {Operator.GREATER, OpGreaterThan},
            {Operator.LESS, OpLessThan},
            {Operator.GR_EQUAL, OpGreaterEqual},
            {Operator.LE_EQUAL, OpLessEqual}
        };

        private IType OpAdd(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value + num.value);
                default:
                    return null;
            }
        }
        private IType OpSubtract(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value - num.value);
                default:
                    return null;
            }
        }
        private IType OpMultiply(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Number(value * num.value);
                case Vector vec:
                    return vec.Operators[Operator.MULTIPLY].Invoke(this);
                default:
                    return null;
            }
        }
        private IType OpDivide(IOperable other)
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
        private IType OpModulo(IOperable other)
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
        private IType OpExponent(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (IsReal && num.IsReal)
                    {
                        return new Number(Math.Pow(value.Real, num.value.Real));
                    }
                    else if (value.Real == 0 && !IsReal && num.IsReal && num.value.Real % 1 == 0)
                    {
                        Complex c;
                        switch (((num.value.Real % 4) + 4) % 4)
                        {
                            case 1:
                                c = new Complex(0, 1);
                                break;
                            case 2:
                                c = new Complex(-1, 0);
                                break;
                            case 3:
                                c = new Complex(0, -1);
                                break;
                            case 0:
                                c = new Complex(1, 0);
                                break;
                        }
                        return new Number(c * Math.Pow(value.Imaginary, num.value.Real));
                    }
                    return new Number(Complex.Pow(value, num.value));
                default:
                    return null;
            }
        }

        private IType OpGreaterThan(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (value.Real != num.value.Real)
                        return (value.Real > num.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                    else
                        return (value.Imaginary > num.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                default:
                    return null;
            }
        }
        private IType OpLessThan(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (value.Real != num.value.Real)
                        return (value.Real < num.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                    else
                        return (value.Imaginary < num.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                default:
                    return null;
            }
        }
        private IType OpGreaterEqual(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (value.Real != num.value.Real)
                        return (value.Real >= num.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                    else
                        return (value.Imaginary >= num.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
                default:
                    return null;
            }
        }
        private IType OpLessEqual(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    if (value.Real != num.value.Real)
                        return (value.Real <= num.value.Real) ? MathBool.TRUE : MathBool.FALSE;
                    else
                        return (value.Imaginary <= num.value.Imaginary) ? MathBool.TRUE : MathBool.FALSE;
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
