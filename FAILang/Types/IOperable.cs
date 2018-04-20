using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public interface IOperable : IType
    {
        Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators { get; }
        Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators { get; }
        Dictionary<UnaryOperator, Func<IType>> UnaryOperators { get; }
    }
}
