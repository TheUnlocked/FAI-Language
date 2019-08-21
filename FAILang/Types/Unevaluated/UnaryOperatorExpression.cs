using FAILang.Types.Unevaluated.Passthrough;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class UnaryOperatorExpression : IUnevaluated
    {
        public string TypeName => "PrefixExpression";

        public UnaryOperator pre;
        public IType target;

        public UnaryOperatorExpression(UnaryOperator pre, IType target)
        {
            this.pre = pre;
            this.target = target;
        }

        public IType Evaluate(Scope scope)
        {
            IType t = target;
            if (target is IUnevaluated utarget)
            {
                t = utarget.Evaluate(scope);
            }
            if (t is Union union)
            {
                IType[] result = new IType[union.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new UnaryOperatorExpression(pre, union.values[i]).Evaluate(scope);
                }
                return new Union(result, scope);
            }
            if (t is IUnevaluated)
                return new BakedExpression(new UnaryOperatorExpression(pre, t), scope);

            // Operate
            if (t is IOperable oper)
            {
                if (oper.UnaryOperators != null && oper.UnaryOperators.TryGetValue(pre, out var action))
                {
                    IType ret = action.Invoke();
                    if (ret == null)
                        return new Error("WrongType", $"The {pre.ToDisplayString()} operator cannot be applied to type {t.TypeName}");
                    return ret;
                }
            }
            return new Error("WrongType", $"The {pre.ToDisplayString()} operator cannot be applied to type {t.TypeName}");
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + pre.GetHashCode();
            hash = hash * 1532528149 + target.GetHashCode();
            return hash;
        }
    }
}
