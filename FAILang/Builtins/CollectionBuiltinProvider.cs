using System;
using System.Collections.Generic;
using System.Text;
using FAILang.Types;

namespace FAILang.Builtins
{
    class CollectionBuiltinProvider : IBuiltinProvider
    {
        private static ExternalFunction ValidateType<T>(Func<T, IType> f, Func<IType, IType> fail) where T : IType
        {
            return x =>
            {
                if (x[0] is T t)
                    return f.Invoke(t);
                return fail.Invoke(x[0]);
            };
        }

        private static ExternFunction LENGTH = new ExternFunction(ValidateType<IIndexable>(
                x => new Number(x.Length),
                x => new Error("WrongType", $"{x} has no length")),
                "c");

        public (string, ExternFunction)[] GetBuiltins() => new(string, ExternFunction)[] {
                ("length", LENGTH)
            };

        public string[] GetReservedNames() => new string[] {
            "length"
        };
    }
}
