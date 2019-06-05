using System.Collections.Generic;
using System.Linq;

namespace FAILang.Types.Unevaluated.Passthrough
{
    public class WhereExpression : IUnevaluated, IPassthrough
    {
        public string TypeName => "WhereExpression";

        public IType PassthroughExpression { get; }

        Dictionary<string, IType> lookups;

        public WhereExpression(IType expression, Dictionary<string, IType> lookups)
        {
            PassthroughExpression = expression;
            this.lookups = lookups;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            if (PassthroughExpression is IUnevaluated u)
            {
                var newLookups = new Dictionary<string, IType>(lookups);
                this.lookups.ToList().ForEach(pair => newLookups[pair.Key] = pair.Value);
                return u.Evaluate(newLookups);
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
    }
}
