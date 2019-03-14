using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types.Unevaluated.Passthrough
{
    interface IPassthrough
    {
        IType PassthroughExpression { get; }
    }
}
