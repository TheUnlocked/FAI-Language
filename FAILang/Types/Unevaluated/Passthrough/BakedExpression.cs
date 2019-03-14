using System.Collections.Generic;

namespace FAILang.Types.Unevaluated.Passthrough
{
    public class BakedExpression : IUnevaluated, IPassthrough
    {
        public string TypeName => "BakedExpression";
        public IType PassthroughExpression { get; }

        Dictionary<string, IType> lookups;

        public BakedExpression(IType expression, Dictionary<string, IType> lookups)
        {
            PassthroughExpression = expression;
            this.lookups = lookups;
        }

        public IType Evaluate(Dictionary<string, IType> _)
        {
            if (PassthroughExpression is IUnevaluated u)
                return u.Evaluate(lookups);
            return PassthroughExpression;
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + PassthroughExpression.GetHashCode();
            hash = hash * 1532528149 + lookups.GetHashCode();
            return hash;
        }
    }
}
