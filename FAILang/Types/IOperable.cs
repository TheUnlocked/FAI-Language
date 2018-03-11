using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public interface IOperable : IType
    {
        Dictionary<Operator, Func<IOperable, IType>> Operators { get; }
    }
}
