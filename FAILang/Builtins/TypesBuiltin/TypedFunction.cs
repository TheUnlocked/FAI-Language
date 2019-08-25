using FAILang.Types;
using FAILang.Types.Unevaluated;
using FAILang.Types.Unevaluated.Passthrough;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FAILang.Builtins
{
    public class TypedFunction : Function
    {
        public Function constructor;

        public TypedFunction(Function constructor, Function func, Scope scope)
            : base(func.fparams, func.expression, scope, func.memoize, func.elipsis)
        {
            this.constructor = constructor;
        }
    }

    public class UnevaluatedTypedFunction : TypedFunction, IUnevaluated
    {
        public UnevaluatedTypedFunction(Function constructor, Function func)
            : base(constructor, func, null)
        {
        }

        public IType Evaluate(Scope scope)
        {
            return new TypedFunction(constructor, this, scope);
        }
    }
}
