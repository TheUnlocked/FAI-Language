using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class PrefixExpression : IUnevaluated
    {
        public string TypeName => "PrefixExpression";

        public Prefix pre;
        public IType target;

        public PrefixExpression(Prefix pre, IType target)
        {
            this.pre = pre;
            this.target = target;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType t = target;
            if (target is IUnevaluated utarget)
            {
                t = utarget.Evaluate(lookups);
            }
            if (t is Union union)
            {
                IType[] result = new IType[union.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new PrefixExpression(pre, union.values[i]).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (t is IUnevaluated)
                return new BakedExpression(new PrefixExpression(pre, t), lookups);

            return pre.Operate(t);
        }
    }
}
