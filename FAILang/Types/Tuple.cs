using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FAILang.Types
{
    class Tuple : IType, IIndexable
    {
        public string TypeName => "Tuple";
        public readonly IType[] items;

        public int Length => items.Length;

        public Tuple(IType[] items)
        {
            this.items = items;
        }

        public IType Index(int index) => items[index];

        public IType IndexRange(int left_b, int right_b)
        {
            IType[] newTuple = new IType[right_b - left_b + 1];
            for (int i = 0; i <= right_b - left_b; i++)
            {
                newTuple[i] = items[i + left_b];
            }
            return new Tuple(newTuple);
        }

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
            if (obj is Tuple v)
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
            return -1319053796 + EqualityComparer<IType[]>.Default.GetHashCode(items);
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

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType[] evalTuple = new IType[items.Length];
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i] is IUnevaluated u)
                {
                    evalTuple[i] = u.Evaluate(lookups);
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
                            tuples[j] = new UnevaluatedTuple(unTuple).Evaluate(lookups);
                        }
                        return new Union(tuples);
                    }
                }
                else
                    evalTuple[i] = items[i];
            }
            return new Tuple(evalTuple);
        }
    }
}
