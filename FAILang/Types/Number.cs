using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FAILang.Types.Unevaluated;

namespace FAILang.Types
{
    public struct Number : IOperable
    {
        public string TypeName => "Number";

        public readonly Complex value;
        public bool IsReal => value.Imaginary == 0;
        public bool IsNatural => IsReal && value.Real % 1 == 0;

        public Number(Complex value)
        {
            this.value = value;
        }

        public Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators => new Dictionary<BinaryOperator, Func<IOperable, IType>>() {
            {BinaryOperator.ADD, OpAdd},
            {BinaryOperator.SUBTRACT, OpSubtract},
            {BinaryOperator.PLUS_MINUS, OpPlusMinus},
            {BinaryOperator.MULTIPLY, OpMultiply},
            {BinaryOperator.DIVIDE, OpDivide},
            {BinaryOperator.EXPONENT, OpExponent},
            {BinaryOperator.CONCAT, OpConcat}
        };

        public Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators => new Dictionary<RelationalOperator, Func<IOperable, MathBool>>() {
            {RelationalOperator.EQUALS, OpEquals},
            {RelationalOperator.NOT_EQUALS, OpNotEquals},
            {RelationalOperator.GREATER, OpGreaterThan},
            {RelationalOperator.GREATER_EQUAL, OpGreaterEqual},
            {RelationalOperator.LESS, OpLessThan},
            {RelationalOperator.LESS_EQUAL, OpLessEqual}
        };

        public Dictionary<UnaryOperator, Func<IType>> UnaryOperators => new Dictionary<UnaryOperator, Func<IType>>()
        {
            {UnaryOperator.NEGATIVE, OpNegate},
            {UnaryOperator.PLUS_MINUS, OpPlusMinus},
            {UnaryOperator.ABS, OpAbs}
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
        private IType OpPlusMinus(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return new Union(new IType[] { OpAdd(num), OpSubtract(num) });
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
                    return vec.BinaryOperators[BinaryOperator.MULTIPLY].Invoke(this);
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
                        return Undefined.Instance;
                    return new Number(value / num.value);
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
                            default:
                                c = Complex.Zero;
                                break;
                        }
                        return new Number(c * Math.Pow(value.Imaginary, num.value.Real));
                    }
                    return new Number(Complex.Pow(value, num.value));
                default:
                    return null;
            }
        }
        private IType OpConcat(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    var log = 1 + Complex.Log10(num.value);
                    Complex floor = new Complex(Math.Floor(log.Real), Math.Floor(log.Imaginary));
                    return new Number(num.value + (value * Complex.Pow(10, floor)));
                default:
                    return null;
            }
        }
        private MathBool OpEquals(IOperable other)
        {
            switch (other)
            {
                case Number num:
                        return Equals(num) ? MathBool.TRUE : MathBool.FALSE;
                default:
                    return null;
            }
        }
        private MathBool OpNotEquals(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    return !Equals(num) ? MathBool.TRUE : MathBool.FALSE;
                default:
                    return null;
            }
        }
        private MathBool OpGreaterThan(IOperable other)
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
        private MathBool OpGreaterEqual(IOperable other)
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
        private MathBool OpLessThan(IOperable other)
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
        private MathBool OpLessEqual(IOperable other)
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
        private IType OpNegate()
        {
            return new Number(-value);
        }
        private IType OpPlusMinus()
        {
            return new Union(new IType[] { new Number(value), OpNegate() });
        }
        private IType OpAbs()
        {
            return new Number(value.Magnitude);
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
