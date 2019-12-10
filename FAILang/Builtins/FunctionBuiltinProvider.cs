using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAILang.Builtins
{
    class FunctionBuiltinProvider : IBuiltinProvider
    {
        public string[] NamespacePath { get; } = new string[] { "std", "function" };

        private static readonly ExternFunction OVERLOAD = new ExternFunction(
            args =>
            {
                Function[] overloads = args.Select(x => x as Function).ToArray();
                if (overloads.Where(x => x == null).Count() > 0)
                {
                    return new Error("WrongType", "All arguments to overload must be functions");
                }
                var (variadic, nonVaridaic) = overloads.SplitBy(x => x.elipsis);
                return new ExternFunction(args2 =>
                {
                    var selectedOverload = nonVaridaic.FirstOrDefault(x => x.fparams.Length == args2.Length);
                    if (selectedOverload == null)
                    {
                        foreach (var overload in variadic.Where(x => x.fparams.Length < args2.Length))
                        {
                            if (selectedOverload == null || selectedOverload.fparams.Length < overload.fparams.Length)
                            {
                                selectedOverload = overload;
                            }
                        }
                        if (selectedOverload == null)
                        {
                            return new Error("ArgumentError", $"No overload supports {args2.Length} arguments.");
                        }
                    }
                    return selectedOverload.Evaluate(args2);
                }, false, true, new string[] { "args" });
            },
            false,
            true,
            "overloads");

        private static IType REDUCEN = BuiltinUtil.FAIValue(@"
            (f, start, n) ->
                ((result, i) -> {self(f(result, i), i + 1) if i < n;
				                 result otherwise;
                )(start, 0)");

        public (string, IType)[] GetBuiltins() => new (string, IType)[] {
            ("overload", OVERLOAD),
            ("reduceN", REDUCEN)
        };
    }
}
