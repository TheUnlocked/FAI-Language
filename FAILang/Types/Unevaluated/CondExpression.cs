using FAILang.Types.Unevaluated.Passthrough;
using System;
using System.Collections;
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

        public IType Evaluate(Scope scope)
        {
            for (int i = 0; i < conds.Length; i++)
            {
                IType tCond = conds[i];
                while (tCond is IUnevaluated u)
                    tCond = u.Evaluate(scope);
                if (tCond is Union tu)
                {
                    return tu.Apply(x => new CondExpression(new IType[] { x }.Concat(conds[(i + 1)..]).ToArray(), exprs[i..], default_expr));
                }
                if (tCond == MathBool.TRUE)
                {
                    IType ret = exprs[i];
                    if (ret is IUnevaluated uexpr)
                        ret = uexpr.Evaluate(scope);
                    if (ret is IUnevaluated)
                    {
                        return new BakedExpression(ret, scope);
                    }
                    return ret;
                }
                if (tCond is Error)
                    return tCond;
            }
            IType retd = default_expr;
            if (retd is IUnevaluated retdu)
            {
                retd = retdu.Evaluate(scope);
            }
            if (retd is IUnevaluated)
            {
                return new BakedExpression(retd, scope);
            }
            return retd;
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + ((IStructuralEquatable)conds).GetHashCode(EqualityComparer<IType>.Default);
            hash = hash * 1532528149 + ((IStructuralEquatable)exprs).GetHashCode(EqualityComparer<IType>.Default);
            hash = hash * 1532528149 + default_expr.GetHashCode();
            return hash;
        }
    }
}
