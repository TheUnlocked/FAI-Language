using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types.Unevaluated
{
    class UnevaluatedFunction : Function, IUnevaluated
    {
        public UnevaluatedFunction(string[] fparams, IType expression, bool memoize = false, bool elipsis = false) :
            base(fparams, expression, null, memoize, elipsis)
        {

        }

        public IType Evaluate(Dictionary<string, IType> lookups) =>
            new Function(fparams, expression, lookups, memoize, elipsis);
    }
}
