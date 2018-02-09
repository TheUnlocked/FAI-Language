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

        public Union(IType[] values, Dictionary<string, IType> lookups = null)
        {
            this.values = values;
            if (lookups != null)
                this.values = Flatten(lookups);
        }

        private IType[] Flatten(Dictionary<string, IType> lookups)
        {
            if (lookups == null)
                lookups = new Dictionary<string, IType>();

            List<IType> newValues = new List<IType>();

            foreach (var type in values)
            {
                var ty = type;
                if (ty is IUnevaluated u && !(ty is Union))
                    ty = u.Evaluate(lookups);
                if (ty is Union)
                {
                    newValues.AddRange((ty as Union).Flatten(lookups));
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

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType[] newVals = new IType[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is IUnevaluated)
                {
                    newVals[i] = (values[i] as IUnevaluated).Evaluate(lookups);
                }
                else {
                    newVals[i] = values[i];
                }
            }
            IType[] evaled = new Union(newVals).Flatten(lookups);
            evaled = evaled.Where(x => x != Void.instance).ToArray();
            if (evaled.Length == 1)
                return evaled[0];
            if (evaled.Length == 0)
                return Void.instance;
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
