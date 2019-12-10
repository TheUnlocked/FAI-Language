using FAILang.Builtins;
using FAILang.Types.Unevaluated;
using FAILang.Types.Unevaluated.Passthrough;
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
        public Scope scope;

        public Function(string[] fparams, IType expression, Scope scope, bool memoize, bool elipsis)
        {
            this.fparams = fparams.ToArray();
            this.expression = expression;
            this.scope = scope;
            this.memoize = memoize;
            this.elipsis = elipsis;
        }

        public virtual IType Evaluate(IType[] args)
        {
            if (args.Length == fparams.Length || (elipsis && args.Length >= fparams.Length - 1))
            {
                if (memoize && memos.TryGetValue(GetArgListHashCode(args), out IType v))
                    return v;
                var newScopeVars = new Dictionary<string, IType>();
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
                        return new Union(results).Evaluate(scope);
                    }
                    if (args[i] is Error)
                        return args[i];
                    if (elipsis && i >= fparams.Length - 1)
                        extra[i - fparams.Length + 1] = args[i];
                    else
                        newScopeVars[fparams[i]] = args[i];
                }
                if (elipsis)
                    newScopeVars[fparams[fparams.Length - 1]] = new UnevaluatedTuple(extra);
                newScopeVars["self"] = this;
                var ret = expression;
                var newScope = new Scope(scope, newScopeVars);
                if (ret is IUnevaluated u)
                    ret = new BakedExpression(u, newScope);
                if (memoize)
                {
                    return new CallbackWrapper(ret, x => memos[GetArgListHashCode(args)] = x);
                }
                return ret;
            }
            return new Error("BadArguments", $"The function {this} can't fit {args.Length} arguments.");
        }

        protected virtual string ExpressionString => "<expression>";

        public override string ToString()
        {
            return ToString(fparams);
        }
        private string ToString(IEnumerable<string> visibleVars)
        {
            string start;
            string expressionString = "";
            string memo = memoize ? (fparams.Length == 0 ? "memo" : "memo ") : "";
            string elipses = elipsis ? "..." : "";
            if (fparams.Length == 1 && !memoize)
                start = $"{fparams[0]}{elipses}";
            else
                start = $"({memo}{string.Join(", ", fparams)}{elipses})";

            if (expression != null)
            {
                Stack<object> exprs = new Stack<object>();
                Stack<object> temp = new Stack<object>();
                IEnumerable<string> tempParams = null;
                exprs.Push(expression);
                while (exprs.Count > 0)
                {
                    switch (exprs.Pop())
                    {
                        case string s:
                            expressionString += s;
                            break;
                        case IEnumerable<string> fparams:
                            tempParams = fparams;
                            continue;
                        case BinaryOperatorExpression e:
                            temp.Push(e.left);
                            temp.Push($" {e.op.ToDisplayString()} ");
                            temp.Push(e.right);
                            break;
                        case UnaryOperatorExpression e:
                            temp.Push(e.pre.ToDisplayString());
                            temp.Push(e.target);
                            break;
                        case RelationalOperatorExpression e:
                            temp.Push(e.ins[0]);
                            int i = 0;
                            while (i < e.ops.Length)
                            {
                                temp.Push($" {e.ops[i].ToDisplayString()} ");
                                temp.Push(e.ins[++i]);
                            }
                            break;
                        case FunctionExpression e:
                            temp.Push(e.func_expr);
                            temp.Push("(");
                            foreach (var arg in e.args)
                            {
                                temp.Push(arg.argument);
                                if (arg.spread)
                                    temp.Push("..");
                                temp.Push(", ");
                            }
                            if (e.args.Length > 0) temp.Pop();
                            temp.Push(")");
                            break;
                        case IndexerExpression e:
                            temp.Push(e.expression);
                            temp.Push("[");
                            if (e.leftIndex != null)
                                temp.Push(e.leftIndex);
                            if (e.range)
                                temp.Push("..");
                            if (e.rightIndex != null)
                                temp.Push(e.rightIndex);
                            temp.Push("]");
                            break;
                        case NamedArgument e:
                            if (!visibleVars.Contains(e.name) && e.name != "self")
                                temp.Push("↑");
                            temp.Push(e.name);
                            break;
                        case CondExpression e:
                            temp.Push("{");
                            for (int j = 0; j < e.exprs.Length; j++)
                            {
                                temp.Push(e.exprs[j]);
                                temp.Push(" if ");
                                temp.Push(e.conds[j]);
                                temp.Push("; ");
                            }
                            if (e.default_expr != Undefined.Instance)
                            {
                                temp.Push(e.default_expr);
                                temp.Push(" otherwise;");
                            }
                            break;
                        case WhereExpression e:
                            temp.Push(e.lookups.Keys);
                            temp.Push(e.PassthroughExpression);
                            temp.Push(" where");
                            foreach (var lookup in e.lookups)
                            {
                                temp.Push($" {lookup.Key} = ");
                                temp.Push(lookup.Value);
                                temp.Push(",");
                            }
                            temp.Push(null);
                            break;
                        case Function e:
                            if (tempParams != null)
                                temp.Push(e.ToString(visibleVars.Concat(e.fparams).Concat(tempParams)));
                            else
                                temp.Push(e.ToString(visibleVars.Concat(e.fparams)));
                            break;
                        case UnevaluatedTuple e:
                            temp.Push("(");
                            switch (e.items.Length)
                            {
                                case 0:
                                    temp.Push(",");
                                    break;
                                case 1:
                                    temp.Push(e.items[0]);
                                    temp.Push(",");
                                    break;
                                default:
                                    foreach (var item in e.items)
                                    {
                                        temp.Push(item);
                                        temp.Push(", ");
                                    }
                                    temp.Pop();
                                    break;
                            }
                            temp.Push(")");
                            break;
                        case UnevaluatedVector e:
                            temp.Push("<");
                            foreach (var item in e.items)
                            {
                                temp.Push(item);
                                temp.Push(", ");
                            }
                            temp.Pop();
                            temp.Push(">");
                            break;
                        case IType v:
                            temp.Push(v.ToString());
                            break;
                        case null:
                            tempParams = null;
                            break;
                        default:
                            break;
                    }
                    while (temp.Count > 0)
                        exprs.Push(temp.Pop());
                }
            }
            else
            {
                expressionString = "<external>";
            }

            return $"{start} -> {expressionString}";
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

    public class UnevaluatedFunction : Function, IUnevaluated
    {
        public UnevaluatedFunction(string[] fparams, IType expression, bool memoize = false, bool elipsis = false) :
            base(fparams, expression, null, memoize, elipsis)
        {

        }

        public IType Evaluate(Scope scope) =>
            new Function(fparams, expression, scope, memoize, elipsis);
    }
}
