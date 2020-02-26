using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using FAILang.Types.Unevaluated.Passthrough;

namespace FAILang.Types.Unevaluated
{
    class FunctionExpression : IUnevaluated
    {
        public string TypeName => "FunctionExpression";

        public (IType argument, bool spread)[] args;
        public IType func_expr = null;

        public FunctionExpression(IType func_expr, (IType, bool)[] args)
        {
            this.func_expr = func_expr;
            this.args = args;
        }

        public IType Evaluate(Scope scope)
        {
            IType func = func_expr;
            if (func is IUnevaluated u)
                func = u.Evaluate(scope);
            if (func is Error)
                return func;

            if (func is Function f && !(func is UnevaluatedFunction))
            {
                List<IType> args = new List<IType>();
                for (int i = 0; i < this.args.Length; i++)
                {
                    var arg = this.args[i].argument;
                    while (arg is IUnevaluated uneval)
                    {
                        arg = uneval.Evaluate(scope);
                    }
                    if (arg is Union un)
                    {   
                        return un.Apply(x => new FunctionExpression(func,
                            args.Select(x => (x, false))
                                .Concat(new[] { (x, this.args[i].spread) })
                                .Concat(this.args[(i + 1)..])
                                .ToArray()));
                    }
                    if (this.args[i].spread)
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
                    return new BakedExpression(new FunctionExpression(func, args.Select(x => (x, false)).ToArray()), scope);
                return f.Evaluate(args.ToArray());
            }
            else if (func is IUnevaluated)
            {
                return new BakedExpression(new FunctionExpression(func, args), scope);
            }
            else if (func is Number n1 && args.Length == 1)
                return new BinaryOperatorExpression(BinaryOperator.MULTIPLY, n1, args[0].argument).Evaluate(scope);
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
