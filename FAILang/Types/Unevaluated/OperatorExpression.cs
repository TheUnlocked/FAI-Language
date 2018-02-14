using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    class OperatorExpression : IUnevaluated
    {
        public string TypeName => "OperatorExpression";

        public Operator op;
        public IType left;
        public IType right;

        public OperatorExpression(Operator op, IType left, IType right)
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
                    result[i] = new OperatorExpression(op, unionLeft.values[i], right).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (right is Union unionRight)
            {
                IType[] result = new IType[unionRight.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new OperatorExpression(op, left, unionRight.values[i]).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (left is IUnevaluated || right is IUnevaluated)
                return new BakedExpression(new OperatorExpression(op, left, right), lookups);
            return op.Operate(left, right);
        }
    }
}
