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

        private static ExternFunction REDUCEN = new ExternFunction(args =>
        {
            IType result = args[1];
            if (args[0] is Function f)
            {
                if (args[2] is Number n && n.IsNatural)
                {
                    for (int i = 0; i < n.value.Real; i++)
                    {
                        result = f.Evaluate(new IType[] { result, new Number(i) });
                    }
                    return result;
                }
                else
                {
                    return new Error("TypeError", $"{args[2]} is not a number.");
                }
            }
            else
            {
                return new Error("TypeError", $"{args[0]} is not a function.");
            }
        }, "accumulator", "start", "n");

        public (string, IType)[] GetBuiltins() => new (string, IType)[] {
            ("overload", OVERLOAD),
            ("reduceN", REDUCEN)
        };
    }
}
