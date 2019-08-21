using FAILang.Types;
using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FAILang.Builtins
{
    public class TypedFunction : Function
    {
        public readonly Function constructor;

        public TypedFunction(Function f, Function constructor, Scope scope)
            : base(f.fparams, f.expression, scope, f.memoize, f.elipsis)
        {
            this.constructor = constructor;
        }
    }

    class UnevaluatedTypedFunction : IUnevaluated
    {
        public Function constructor;
        public Function thisFunction;
        public string TypeName => "UnevaluatedTypedFunction";

        public UnevaluatedTypedFunction(Function f, Function constructor=null)
        {
            this.constructor = constructor;
            thisFunction = f;
        }

        public IType Evaluate(Scope scope) =>
            new TypedFunction(thisFunction, constructor, scope);
    }
}
