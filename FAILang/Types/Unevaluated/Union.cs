using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FAILang.Types.Unevaluated
{
    public class Union : IUnevaluated
    {
        public string TypeName => "Union";

        public IType[] values;

        public Union(IType[] values, Scope scope = null)
        {
            this.values = values.Where(x => x != Undefined.Instance).ToArray();
            if (this.values.Length == 0)
                this.values = new IType[] { Undefined.Instance };
            if (scope != null)
                this.values = Flatten(scope);
        }

        private IType[] Flatten(Scope scope)
        {
            List<IType> newValues = new List<IType>();

            foreach (var type in values)
            {
                var ty = type;
                if (ty is IUnevaluated u && !(ty is Union))
                    ty = u.Evaluate(scope);
                if (ty is Union)
                {
                    newValues.AddRange((ty as Union).Flatten(scope));
                }
                else
                {
                    newValues.Add(ty);
                }
            }

            return newValues.Distinct().ToArray();
        }

        public override string ToString()
        {
            return $"({string.Join(" | ", values.Select(v => v.ToString()))})";
        }

        public IType Evaluate(Scope scope)
        {
            IType[] newVals = new IType[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is IUnevaluated)
                {
                    newVals[i] = (values[i] as IUnevaluated).Evaluate(scope);
                }
                else {
                    newVals[i] = values[i];
                }
            }
            IType[] evaled = new Union(newVals).Flatten(scope);
            evaled = evaled.Where(x => x != Undefined.Instance).ToArray();
            if (evaled.Length == 1)
                return evaled[0];
            if (evaled.Length == 0)
                return Undefined.Instance;
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
