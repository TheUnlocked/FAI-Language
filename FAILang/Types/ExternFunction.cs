using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    public delegate IType ExternalFunction(params IType[] ins);

    public class ExternFunction : Function
    {
        ExternalFunction func;

        public ExternFunction(ExternalFunction func, params string[] fparams) : base(fparams, null, null, false, false)
        {
            this.func = func;
        }

        public ExternFunction(ExternalFunction func, bool memoize = false, bool elipses = false, params string[] fparams) : base(fparams, null, null, memoize, elipses)
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
            return new Error("BadArguments", $"The function {this} can't fit {args.Length} arguments.");
        }

        protected override string ExpressionString => "<external>";
    }
}
