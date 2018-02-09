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

        public Expression[] conds;
        public Expression[] exprs;
        public Expression default_expr;

        public CondExpression(Expression[] conds, Expression[] exprs, Expression default_expr)
        {
            this.conds = conds;
            this.exprs = exprs;
            this.default_expr = default_expr;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            for (int i = 0; i < conds.Length; i++)
            {
                if (conds[i].Evaluate(lookups) == MathBool.TRUE)
                {
                    return exprs[i].Evaluate(lookups);
                }
            }
            return default_expr.Evaluate(lookups);
        }
    }
}
