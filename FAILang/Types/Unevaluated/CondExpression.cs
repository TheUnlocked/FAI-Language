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
                if ((conds[i] is IUnevaluated u ? u.Evaluate(lookups) : conds[i]) == MathBool.TRUE)
                {
                    return exprs[i] is IUnevaluated retu ? retu.Evaluate(lookups) : exprs[i];
                }
            }
            return default_expr is IUnevaluated retd ? retd.Evaluate(lookups) : default_expr;
        }
    }
}
