using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            if (func is IPopup)
                return func;

            if (func is Function f) {
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
                        if(arg is Tuple tu)
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
                return f.Evaluate(args.ToArray());
            }
            else if (func is Number n1 && args.Length == 1)
                if (args[0].Item1 is Number n2)
                    return new Number(n1.value * n2.value);
                else
                    return new Error("WrongType", $"The * operator cannot be applied to types {func.TypeName} and {args[0].Item1.TypeName}");
            return new Error("SyntaxError", $"You can't call an object of type {func.TypeName}.");
        }
    }
}
