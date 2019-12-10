using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FAILang.Types.Unevaluated;

namespace FAILang.Types
{
    public class Union : IType
    {
        public string TypeName => "Union";

        public IType[] values;

        public Union(IType[] values)
        {
            this.values = values;
        }

        public IType Apply(Func<IType, IType> f)
        {
            IType[] newValues = new IType[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                newValues[i] = f(values[i]);
            }
            return new UnevaluatedUnion(newValues);
        }

        public override string ToString()
        {
            return $"({string.Join(" | ", values.Select(v => v.ToString()))})";
        }
    }

    public class UnevaluatedUnion : IUnevaluated
    {
        public string TypeName => "UnevaluatedUnion";

        public IType[] values;

        public UnevaluatedUnion(IType[] values)
        {
            this.values = values.Where(x => x != Undefined.Instance).ToArray();
        }

        private IType[] Flatten(Scope scope)
        {
            List<IType> newValues = new List<IType>();

            foreach (var type in values)
            {
                var ty = type;
                while (ty is IUnevaluated u)
                    ty = u.Evaluate(scope);
                if (ty is Union)
                {
                    newValues.AddRange((ty as Union).values);
                }
                else
                {
                    newValues.Add(ty);
                }
            }

            return newValues.Distinct().ToArray();
        }

        public IType Evaluate(Scope scope)
        {
            IType[] evaled = Flatten(scope);
            evaled = evaled.Where(x => x != Undefined.Instance).ToArray();
            if (evaled.Length == 0)
                return Undefined.Instance;
            if (evaled.Length == 1)
                return evaled[0];
            foreach (IType e in evaled)
            {
                if (e is Error)
                    return e;
            }
            return new Union(evaled);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 1649527923 + EqualityComparer<IType[]>.Default.GetHashCode(values);
        }
    }
}
