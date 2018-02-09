using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    public class Expression : IUnevaluated
    {
        public string TypeName => "UnevaluatedExpression";

        public IType value;

        protected Expression()
        {

        }

        public Expression(IType value)
        {
            this.value = value;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            if (value is IUnevaluated)
            {
                return (value as IUnevaluated).Evaluate(lookups);
            }
            return value;
        }
    }
}
