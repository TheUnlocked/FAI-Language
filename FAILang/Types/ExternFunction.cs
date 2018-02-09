using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public delegate IType ExternalFunction(params IType[] ins);

    class ExternFunction : Function
    {
        ExternalFunction func;

        public ExternFunction(ExternalFunction func, params string[] fparams) : base(fparams, null)
        {
            this.func = func;
        }

        public override IType Evaluate(IType[] args)
        {
            if (args.Length == fparams.Length)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is Union un)
                    {
                        IType[] results = new IType[un.values.Length];
                        for (int j = 0; j < results.Length; j++)
                        {
                            IType[] newArgs = new IType[args.Length];
                            for (int k = 0; k < newArgs.Length; k++)
                            {
                                if (i == k)
                                    newArgs[k] = un.values[j];
                                else
                                    newArgs[k] = args[k];
                            }
                            results[j] = Evaluate(newArgs);
                        }
                        return new Union(results);
                    }
                }
                return func(args);
            }
            return new Error("BadArguments", $"The function lambda({string.Join(", ", fparams)}: <external>) can't fit {args.Length} arguments.");
        }

        public override string ToString()
        {
            return $"lambda({string.Join(", ", fparams)}: <external>)";
        }
    }
}
