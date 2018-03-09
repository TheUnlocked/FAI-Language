using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    public class Function : IType
    {
        public string TypeName => "Function";

        public readonly string[] fparams;
        public readonly bool elipsis;
        public readonly IType expression;

        public bool memoize;
        public Dictionary<int, IType> memos = new Dictionary<int, IType>();

        public Function(string[] fparams, IType expression, bool memoize = false, bool elipsis = false)
        {
            this.fparams = fparams.ToArray();
            this.expression = expression;
            this.memoize = memoize;
            this.elipsis = elipsis;
        }

        public virtual IType Evaluate(IType[] args)
        {
            if (args.Length == fparams.Length || (elipsis && args.Length >= fparams.Length - 1))
            {
                if (memoize && memos.TryGetValue(GetArgListHashCode(args), out IType v))
                    return v;
                Dictionary<string, IType> lookup = new Dictionary<string, IType>();
                IType[] extra = new IType[args.Length - fparams.Length + 1];
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
                        return new Union(results).Evaluate(lookup);
                    }
                    if (args[i] is Error)
                        return args[i];
                    if (elipsis && i >= fparams.Length - 1)
                        extra[i - fparams.Length + 1] = args[i];
                    else
                        lookup[fparams[i]] = args[i];
                }
                if (elipsis)
                    lookup[fparams[fparams.Length - 1]] = new UnevaluatedTuple(extra);
                lookup["self"] = this;
                var ret = expression;
                if (ret is IUnevaluated u)
                    ret = new BakedExpression(u, lookup);
                if (memoize)
                    return new CallbackWrapper(ret, x => memos[GetArgListHashCode(args)] = x);
                return ret;
            }
            return new Error("BadArguments", $"The function {this} can't fit {args.Length} arguments.");
        }

        protected virtual string ExpressionString => "<expression>";

        public override string ToString()
        {
            string memo = memoize ? (fparams.Length == 0 ? "memo" : "memo ") : "";
            string elipses = elipsis ? "..." : "";
            if (fparams.Length == 1 && !memoize)
                return $"{fparams[0]}{elipses} -> {ExpressionString}";
            else
                return $"({memo}{string.Join(", ", fparams)}{elipses}) -> {ExpressionString}";
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hashCode = 315160578;
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(fparams);
            hashCode = hashCode * -1521134295 + EqualityComparer<IType>.Default.GetHashCode(expression);
            return hashCode;
        }

        public int GetArgListHashCode(IType[] args)
        {
            var hashCode = 55313;
            for (int i = 0; i < args.Length; i++)
            {
                hashCode = hashCode * 32119 + args[i].GetHashCode();
            }
            return hashCode;
        }
    }
}
