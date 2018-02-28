using System.Collections.Generic;

namespace FAILang.Types.Unevaluated
{
    public class BakedExpression : IUnevaluated
    {
        public string TypeName => "BakedExpression";

        IType expression;
        Dictionary<string, IType> lookups;

        public BakedExpression(IType expression, Dictionary<string, IType> lookups)
        {
            this.expression = expression;
            this.lookups = lookups;
        }

        public IType Evaluate(Dictionary<string, IType> _)
        {
            if (expression is IUnevaluated u)
                return u.Evaluate(lookups);
            return expression;
        }
    }
}
