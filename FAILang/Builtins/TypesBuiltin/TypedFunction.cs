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

        public TypedFunction(Function f, Function constructor, Dictionary<string, IType> lookups=null)
            : base(f.fparams, f.expression, lookups ?? f.lookups ?? new Dictionary<string, IType>(), f.memoize, f.elipsis)
        {
            this.constructor = constructor;
        }
    }

    class UnevaluatedTypedFunction : TypedFunction, IUnevaluated
    {
        public Function Constructor { get; internal set; }

        public UnevaluatedTypedFunction(Function f, Function constructor=null) :
            base(f, null)
        {
            Constructor = constructor;
        }

        public IType Evaluate(Dictionary<string, IType> lookups) =>
            new TypedFunction(this, Constructor, lookups);
    }
}
