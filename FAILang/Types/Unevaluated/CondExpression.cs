using FAILang.Types.Unevaluated.Passthrough;
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
                IType tCond = conds[i];
                if (tCond is IUnevaluated u)
                    tCond = u.Evaluate(lookups);
                if (tCond is Union tu)
                {
                    IType[] result = new IType[tu.values.Length];
                    for (int j = 0; j < result.Length; j++)
                    {
                        var ncond = new CondExpression(conds.Skip(i).ToArray(), exprs.Skip(i).ToArray(), default_expr);
                        ncond.conds[0] = tu.values[j];
                        result[j] = ncond.Evaluate(lookups);
                    }
                    return new Union(result, lookups);
                }
                if (tCond is IUnevaluated)
                {
                    var nexpr = new CondExpression(conds.Skip(i).ToArray(), exprs.Skip(i).ToArray(), default_expr);
                    nexpr.conds[0] = tCond;
                    return new BakedExpression(nexpr, lookups);
                }
                if (tCond == MathBool.TRUE)
                {
                    IType ret = exprs[i];
                    if (ret is IUnevaluated uexpr)
                        ret = uexpr.Evaluate(lookups);
                    if (ret is IUnevaluated && !(ret is Union))
                    {
                        return new BakedExpression(ret, lookups);
                    }
                    return ret;
                }
                if (tCond is Error)
                    return tCond;
            }
            IType retd = default_expr;
            if (retd is IUnevaluated retdu)
            {
                retd = retdu.Evaluate(lookups);
            }
            if (retd is IUnevaluated && !(retd is Union))
            {
                return new BakedExpression(retd, lookups);
            }
            return retd;
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + EqualityComparer<IType[]>.Default.GetHashCode(conds);
            hash = hash * 1532528149 + EqualityComparer<IType[]>.Default.GetHashCode(exprs);
            hash = hash * 1532528149 + default_expr.GetHashCode();
            return hash;
        }
    }
}
