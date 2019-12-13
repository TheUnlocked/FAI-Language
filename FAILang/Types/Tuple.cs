using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FAILang.Types.Unevaluated.Passthrough;
using System.Collections;

namespace FAILang.Types
{
    public struct Tuple : IIndexable, IOperable
    {
        public string TypeName => "Tuple";
        public readonly IType[] items;

        public int Length => items.Length;

        public Tuple(IType[] items)
        {
            this.items = items;
        }

        public Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators => new Dictionary<BinaryOperator, Func<IOperable, IType>>()
        {
            {BinaryOperator.CONCAT, OpConcat}
        };

        public Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators => null;
        public Dictionary<UnaryOperator, Func<IType>> UnaryOperators => null;

        private IType OpConcat(IOperable other)
        {
            switch (other)
            {
                case Tuple tup:
                    return new Tuple(items.Concat(tup.items).ToArray());
                default:
                    return null;
            }
        }

        public IType Index(int index) => items[index];

        public IType IndexRange(int left_b, int right_b) => new Tuple(items.Skip(left_b).SkipLast(Length - right_b - 1).ToArray());

        public override string ToString() {
            switch (items.Length) {
                case 0:
                    return "(,)";
                case 1:
                    return $"({items[0]},)";
                default:
                    return $"({String.Join(", ", (object[])items)})";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Tuple t)
            {
                if (t.Length != Length)
                    return false;
                for (int i = 0; i < t.Length; i++)
                    if (!items[i].Equals(t.items[i]))
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

    class UnevaluatedTuple : IUnevaluated
    {
        public string TypeName => "UnevaluatedTuple";

        public readonly IType[] items;

        public UnevaluatedTuple(IType[] items)
        {
            this.items = items;
        }

        public IType Evaluate(Scope scope)
        {
            IType[] evalTuple = new IType[items.Length];
            bool incomplete = false;
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i] is IUnevaluated u)
                {
                    evalTuple[i] = u.Evaluate(scope);
                    if (evalTuple[i] is Union un)
                    {
                        for (int k = i; k < items.Length; k++)
                        {
                            evalTuple[k] = items[k];
                        }
                        IType[] tuples = new IType[un.values.Length];
                        for (int j = 0; j < tuples.Length; j++)
                        {
                            var unTuple = evalTuple.ToArray();
                            unTuple[i] = un.values[j];
                            tuples[j] = new UnevaluatedTuple(unTuple).Evaluate(scope);
                        }
                        return new Union(tuples);
                    }
                    if (evalTuple[i] is IUnevaluated)
                        incomplete = true;
                }
                else
                    evalTuple[i] = items[i];
            }
            if (incomplete)
            {
                return new BakedExpression(new UnevaluatedTuple(evalTuple), scope);
            }
            return new Tuple(evalTuple);
        }
    }
}
