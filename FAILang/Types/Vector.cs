using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FAILang.Types
{
    struct Vector : IType, IIndexable
    {
        public string TypeName => "Vector";
        public readonly IType[] items;

        public int Length => items.Length;

        public Vector(IType[] items)
        {
            this.items = items;
        }

        public IType Index(int index) => items[index];

        public IType IndexRange(int left_b, int right_b)
        {
            IType[] newVector = new IType[right_b - left_b + 1];
            for (int i = 0; i <= right_b - left_b; i++)
            {
                newVector[i] = items[i + left_b];
            }
            return new Vector(newVector);
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
            return -1319053796 + EqualityComparer<IType[]>.Default.GetHashCode(items);
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

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType[] evalVector = new IType[items.Length];
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i] is IUnevaluated u)
                {
                    evalVector[i] = u.Evaluate(lookups);
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
                            vectors[j] = new UnevaluatedVector(unVector).Evaluate(lookups);
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
