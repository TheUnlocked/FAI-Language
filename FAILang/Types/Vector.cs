using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    class Vector : IType, IIndexable
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
                    evalVector[i] = u.Evaluate(lookups);
                else
                    evalVector[i] = items[i];
            }
            return new Vector(evalVector);
        }
    }
}
