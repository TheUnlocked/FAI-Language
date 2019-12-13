using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections;

namespace FAILang.Types
{
    public struct Vector : IOperable, IIndexable
    {
        public string TypeName => "Vector";
        public readonly IType[] items;

        public int Length => items.Length;

        public Vector(IType[] items)
        {
            this.items = items;
        }

        public Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators => new Dictionary<BinaryOperator, Func<IOperable, IType>>() {
            {BinaryOperator.ADD, OpAdd},
            {BinaryOperator.SUBTRACT, OpSubtract},
            {BinaryOperator.PLUS_MINUS, OpPlusMinus},
            {BinaryOperator.MULTIPLY, OpMultiply},
            {BinaryOperator.CONCAT, OpConcat}
        };

        public Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators => null;

        public Dictionary<UnaryOperator, Func<IType>> UnaryOperators => new Dictionary<UnaryOperator, Func<IType>>()
        {
            {UnaryOperator.NEGATIVE, OpNegate},
            {UnaryOperator.PLUS_MINUS, OpPlusMinus},
            {UnaryOperator.ABS, OpMagnitude}
        };

        private IType OpAdd(IOperable other)
        {
            switch (other)
            {
                case Vector vec:
                    if (Length != vec.Length)
                        return new Error("DimensionMismatch", "Vectors of different sizes cannot be added");
                    IType[] ret = new IType[Length];
                    for (int i = 0; i < Length; i++) {
                        ret[i] = new BinaryOperatorExpression(BinaryOperator.ADD, items[i], vec.items[i]).Evaluate(null);
                        if (ret[i] is Error)
                            return ret[i];
                    }
                    return new Vector(ret);
                default:
                    return null;
            }
        }
        private IType OpSubtract(IOperable other)
        {
            switch (other)
            {
                case Vector vec:
                    if (Length != vec.Length)
                        return new Error("DimensionMismatch", "Vectors of different sizes cannot be subtracted");
                    IType[] ret = new IType[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        ret[i] = new BinaryOperatorExpression(BinaryOperator.SUBTRACT, items[i], vec.items[i]).Evaluate(null);
                        if (ret[i] is Error)
                            return ret[i];
                    }
                    return new Vector(ret);
                default:
                    return null;
            }
        }
        private IType OpPlusMinus(IOperable other)
        {
            var pos = OpAdd(other);
            var neg = OpSubtract(other);
            if (pos == null || neg == null)
            {
                return null;
            }
            if (pos is Error) return pos;
            if (neg is Error) return neg;
            return new UnevaluatedUnion(new IType[] { pos, neg });
        }
        private IType OpPlusMinus()
        {
            var neg = OpNegate();
            if (neg is Error)
            {
                return neg;
            }
            return new UnevaluatedUnion(new IType[] { new Vector(items), neg });
        }
        private IType OpMultiply(IOperable other)
        {
            switch (other)
            {
                case Number num:
                    IType[] ret = new IType[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        ret[i] = new BinaryOperatorExpression(BinaryOperator.MULTIPLY, items[i], num).Evaluate(null);
                        if (ret[i] is Error)
                            return ret[i];
                    }
                    return new Vector(ret);

                case Vector vec:
                    if (Length != vec.Length)
                        return new Error("DimensionMismatch", "Vectors of different sizes cannot be added");
                    IType total = null;
                    for (int i = 0; i < Length; i++)
                    {
                        var res = new BinaryOperatorExpression(BinaryOperator.MULTIPLY, items[i], vec.items[i]).Evaluate(null);
                        if (res is Error)
                            return res;
                        if (total == null)
                            total = res;
                        else
                            total = new BinaryOperatorExpression(BinaryOperator.ADD, total, res).Evaluate(null);
                    }
                    return total;
                default:
                    return null;
            }
        }
        private IType OpConcat(IOperable other)
        {
            switch (other)
            {
                case Vector vec:
                    return new Vector(items.Concat(vec.items).ToArray());
                default:
                    return null;
            }
        }
        private IType OpNegate()
        {
            IType[] newItems = new IType[items.Length];
            for (int i = 0; i < newItems.Length; i++) {
                if (items[i] is Number num)
                {
                    newItems[i] = new Number(-num.value);
                }
                else
                    return new Error("WrongType", $"Vector inputs to the - operator must be purely numeric");
            }
            return new Vector(newItems);
        }
        private IType OpMagnitude()
        {
            Complex sum = 0;
            foreach (var t in items)
            {
                if (t is Number num)
                {
                    sum += num.value * num.value;
                }
                else
                    return new Error("WrongType", $"Vector inputs to the |x| operator must be purely numeric");
            }
            return new Number(Complex.Sqrt(sum));
        }

        public IType Index(int index) => items[index];
        public IType IndexRange(int left_b, int right_b) {
            var newItems = items.Skip(left_b).SkipLast(Length - right_b - 1).ToArray();
            if (newItems.Length > 0)
            {
                return new Vector(newItems);
            }
            return Undefined.Instance;
        }

        public override string ToString() => $"<{String.Join(", ", (object[])items)}>";

        public override bool Equals(object obj)
        {
            if (obj is Vector v)
            {
                if (v.Length != Length)
                    return false;
                for (int i = 0; i < v.Length; i++)
                    if (!items[i].Equals(v.items[i]))
                        return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return -1319053796 + ((IStructuralEquatable)items).GetHashCode(EqualityComparer<IType>.Default);
        }
    }

    class UnevaluatedVector : IUnevaluated
    {
        public string TypeName => "UnevaluatedVector";

        public readonly IType[] items;

        public UnevaluatedVector(IType[] items)
        {
            this.items = items;
        }

        public IType Evaluate(Scope scope)
        {
            IType[] evalVector = new IType[items.Length];
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i] is IUnevaluated u)
                {
                    evalVector[i] = u.Evaluate(scope);
                    if (evalVector[i] is Union un)
                    {
                        for (int k = i; k < items.Length; k++)
                        {
                            evalVector[k] = items[k];
                        }
                        IType[] vectors = new IType[un.values.Length];
                        for (int j = 0; j < vectors.Length; j++)
                        {
                            var unVector = evalVector.ToArray();
                            unVector[i] = un.values[j];
                            vectors[j] = new UnevaluatedVector(unVector).Evaluate(scope);
                        }
                        return new Union(vectors);
                    }
                }
                else
                    evalVector[i] = items[i];
            }
            return new Vector(evalVector);
        }
    }
}
