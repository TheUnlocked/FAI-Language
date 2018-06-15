using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FAILang.Types.Unevaluated
{
    class FunctionExpression : IUnevaluated
    {
        public string TypeName => "FunctionExpression";

        public (IType, bool)[] args;
        public IType func_expr = null;

        public FunctionExpression(IType func_expr, (IType, bool)[] args)
        {
            this.func_expr = func_expr;
            this.args = args;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType func = func_expr;
            if (func is IUnevaluated u)
                func = u.Evaluate(lookups);
            if (func is Error)
                return func;

            if (func is Function f && !(func is UnevaluatedFunction))
            {
                List<IType> args = new List<IType>();
                for (int i = 0; i < this.args.Length; i++)
                {
                    var arg = this.args[i].Item1;
                    if (arg is IUnevaluated uneval)
                    {
                        arg = uneval.Evaluate(lookups);
                    }
                    if (this.args[i].Item2)
                    {
                        if (arg is Tuple tu)
                        {
                            for (int j = 0; j < tu.items.Length; j++)
                            {
                                args.Add(tu.items[j]);
                            }
                        }
                        else
                        {
                            return new Error("NotExpandable", $"An object of type {arg.TypeName} cannot be expanded into arguments via tuple expansion.");
                        }
                    }
                    else
                    {
                        args.Add(arg);
                    }
                }
                if (args.Any(x => x is IUnevaluated))
                    return new BakedExpression(new FunctionExpression(func, args.Select(x => (x, false)).ToArray()), lookups);
                return f.Evaluate(args.ToArray());
            }
            if (func is IUnevaluated)
            {
                return new BakedExpression(new FunctionExpression(func, args), lookups);
            }
            if (func is IOperable n1 && args.Length == 1)
                return new BinaryOperatorExpression(BinaryOperator.MULTIPLY, n1, args[0].Item1).Evaluate(lookups);
            return new Error("SyntaxError", $"You can't call an object of type {func.TypeName}.");
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + EqualityComparer<(IType, bool)[]>.Default.GetHashCode(args);
            hash = hash * 1532528149 + func_expr.GetHashCode();
            return hash;
        }
    }
}
