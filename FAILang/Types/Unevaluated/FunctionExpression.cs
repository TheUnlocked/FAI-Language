using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class FunctionExpression : IUnevaluated
    {
        public string TypeName => "FunctionExpression";

        public string fname = null;
        public IType[] args;
        public IType func_expr = null;

        public FunctionExpression(IType func_expr, IType[] args)
        {
            this.func_expr = func_expr;
            this.args = args;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType func = func_expr;
            if (func is IUnevaluated u)
                func  = u.Evaluate(lookups);
            if (func is IPopup)
                return func;

            IType[] args = new IType[this.args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (this.args[i] is IUnevaluated)
                {
                    args[i] = (this.args[i] as IUnevaluated).Evaluate(lookups);
                }
                else
                {
                    args[i] = this.args[i];
                }
            }
            if (func is Function f)
                return f.Evaluate(args);
            else if (func is Number n1 && args.Length == 1)
                if (args[0] is Number n2)
                    return new Number(n1.value * n2.value);
                else
                    return new Error("WrongType", $"The * operator cannot be applied to types {func.TypeName} and {args[0].TypeName}");
            return new Error("SyntaxError", $"You can't call an object of type {func.TypeName}.");
        }
    }

    public class BakedExpression : IUnevaluated
    {
        public string TypeName => "BakedExpression";

        IType expression;
        Dictionary<string, IType> lookups;

        public BakedExpression(IType expression, Dictionary<string, IType> lookups)
        {
            this.expression = expression;
            this.lookups = lookups;
        }

        public IType Evaluate(Dictionary<string, IType> _)
        {
            if (expression is IUnevaluated u)
                return u.Evaluate(lookups);
            return expression;
        }
    }
}
