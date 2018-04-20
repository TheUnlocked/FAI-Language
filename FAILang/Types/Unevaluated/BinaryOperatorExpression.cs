using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class BinaryOperatorExpression : IUnevaluated
    {
        public string TypeName => "OperatorExpression";

        public BinaryOperator op;
        public IType left;
        public IType right;

        public BinaryOperatorExpression(BinaryOperator op, IType left, IType right)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            var left = this.left;
            var right = this.right;
            if (left is IUnevaluated uLeft)
                left = uLeft.Evaluate(lookups);
            if (right is IUnevaluated uRight)
                right = uRight.Evaluate(lookups);
            if (left is Union unionLeft)
            {
                IType[] result = new IType[unionLeft.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new BinaryOperatorExpression(op, unionLeft.values[i], right).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (right is Union unionRight)
            {
                IType[] result = new IType[unionRight.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new BinaryOperatorExpression(op, left, unionRight.values[i]).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (left is IUnevaluated || right is IUnevaluated)
                return new BakedExpression(new BinaryOperatorExpression(op, left, right), lookups);

            if (left is Error eLeft)
                return eLeft;
            if (right is Error eRight)
                return eRight;

            // Operate
            if (left == Undefined.instance || right == Undefined.instance)
                    return Undefined.instance;

            if (left is IOperable lop && right is IOperable rop)
            {
                if (lop.BinaryOperators != null && rop.BinaryOperators != null && lop.BinaryOperators.TryGetValue(op, out var lac) && rop.BinaryOperators.ContainsKey(op))
                {
                    IType ret = lac.Invoke(rop);
                    if (ret == null)
                        return new Error("WrongType", $"The {op.ToDisplayString()} operator cannot be applied to types {left.TypeName} and {right.TypeName}");
                    return ret;
                }
            }
            return new Error("WrongType", $"The {op.ToDisplayString()} operator cannot be applied to types {left.TypeName} and {right.TypeName}");
        }
    }
}
