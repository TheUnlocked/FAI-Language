using System.Collections.Generic;

namespace FAILang.Types.Unevaluated.Passthrough
{
    public class BakedExpression : IUnevaluated, IPassthrough
    {
        public string TypeName => "BakedExpression";
        public IType PassthroughExpression { get; }

        Scope scope;

        public BakedExpression(IType expression, Scope scope)
        {
            PassthroughExpression = expression;
            this.scope = scope;
        }

        public IType Evaluate(Scope _)
        {
            if (PassthroughExpression is IUnevaluated u)
                return u.Evaluate(scope);
            return PassthroughExpression;
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + PassthroughExpression.GetHashCode();
            hash = hash * 1532528149 + scope.GetHashCode();
            return hash;
        }

        public IType ReplacePassthroughExpression(IType replacement)
        {
            return new BakedExpression(replacement, scope);
        }
    }
}
