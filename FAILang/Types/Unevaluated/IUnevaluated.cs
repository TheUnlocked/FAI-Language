using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    interface IUnevaluated : IType
    {
        IType Evaluate(Scope scope);
    }
}
