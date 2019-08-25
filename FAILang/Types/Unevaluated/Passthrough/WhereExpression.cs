using System.Collections.Generic;
using System.Linq;

namespace FAILang.Types.Unevaluated.Passthrough
{
    public class WhereExpression : IUnevaluated, IPassthrough
    {
        public string TypeName => "WhereExpression";

        public IType PassthroughExpression { get; }

        public Dictionary<string, IType> lookups;

        public WhereExpression(IType expression, Dictionary<string, IType> lookups)
        {
            PassthroughExpression = expression;
            this.lookups = lookups;
        }

        public IType Evaluate(Scope scope)
        {
            if (PassthroughExpression is IUnevaluated u)
            {
                var newScope = new Scope(scope, lookups);
                return u.Evaluate(newScope);
            }
            return PassthroughExpression;
        }

        public override int GetHashCode()
        {
            int hash = 691949119;
            hash = hash * 1532528149 + PassthroughExpression.GetHashCode();
            hash = hash * 1532528149 + lookups.GetHashCode();
            return hash;
        }

        public IType ReplacePassthroughExpression(IType replacement)
        {
            return new WhereExpression(replacement, lookups);
        }
    }
}
