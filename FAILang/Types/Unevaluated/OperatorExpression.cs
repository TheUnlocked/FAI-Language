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
            var uRight = right as IUnevaluated;
            if (left is IUnevaluated uLeft)
            {
                left = uLeft.Evaluate(lookups);
                Union unionLeft = left as Union;
                if (uRight != null)
                {
                    right = uRight.Evaluate(lookups);
                    if (unionLeft != null)
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
                    return op.Operate(left, right);
                }
                if (unionLeft != null)
                {
                    IType[] result = new IType[unionLeft.values.Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = new OperatorExpression(op, unionLeft.values[i], right).Evaluate(lookups);
                    }
                    return new Union(result, lookups);
                }
                return op.Operate(left, right);
            }
            else if (uRight != null)
            {
                right = uRight.Evaluate(lookups);
                if (right is Union unionRight)
                {
                    IType[] result = new IType[unionRight.values.Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = new OperatorExpression(op, left, unionRight.values[i]).Evaluate(lookups);
                    }
                    return new Union(result, lookups);
                }
                return op.Operate(left, right);
            }
            return op.Operate(left, right);
        }
    }
}
