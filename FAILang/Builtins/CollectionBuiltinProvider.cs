using System;
using System.Collections.Generic;
using System.Text;
using FAILang.Types;

namespace FAILang.Builtins
{
    class CollectionBuiltinProvider : IBuiltinProvider
    {
        public string[] NamespacePath { get; } = new string[] { "std", "collections" };

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

        public (string, IType)[] GetBuiltins() => new(string, IType)[] {
                ("length", LENGTH)
            };

        public string[] GetReservedNames() => new string[] {
            "length"
        };
    }
}
