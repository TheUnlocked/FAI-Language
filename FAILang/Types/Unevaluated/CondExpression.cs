using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class CondExpression : IUnevaluated
    {
        public string TypeName => "CondExpression";

        public IType[] conds;
        public IType[] exprs;
        public IType default_expr;

        public CondExpression(IType[] conds, IType[] exprs, IType default_expr)
        {
            this.conds = conds;
            this.exprs = exprs;
            this.default_expr = default_expr;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            for (int i = 0; i < conds.Length; i++)
            {
                IType t = conds[i];
                if (t is IUnevaluated u)
                    t = u.Evaluate(lookups);
                if (t is IUnevaluated)
                {
                    var nexpr = new CondExpression(conds.Skip(i).ToArray(), exprs.Skip(i).ToArray(), default_expr);
                    nexpr.conds[0] = t;
                    return new BakedExpression(nexpr, lookups);
                }
                if (t == MathBool.TRUE)
                {
                    IType ret = exprs[i];
                    if (ret is IUnevaluated uexpr)
                        ret = uexpr.Evaluate(lookups);
                    if (ret is IUnevaluated)
                    {
                        var nexpr = new CondExpression(conds.Skip(i).ToArray(), exprs.Skip(i).ToArray(), default_expr);
                        nexpr.exprs[0] = ret;
                        return new BakedExpression(nexpr, lookups);
                    }
                    return ret;
                }
            }
            return default_expr is IUnevaluated retd ? retd.Evaluate(lookups) : default_expr;
        }
    }
}
