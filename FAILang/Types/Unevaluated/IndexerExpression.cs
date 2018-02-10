using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types.Unevaluated
{
    class IndexerExpression : IUnevaluated
    {
        public string TypeName => "IndexerExpression";

        public IType expression;
        public IType leftIndex;
        public IType rightIndex;
        public bool range;

        public IndexerExpression(IType expression, IType left, IType right, bool elipsis)
        {
            this.expression = expression;
            leftIndex = left;
            rightIndex = right;
            range = elipsis;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType expr = expression;
            if (expr is IUnevaluated u)
            {
                expr = u.Evaluate(lookups);
            }
            if (expr is IIndexable item)
            {
                int left = 0;
                int right = item.Length - 1;

                IType t_left = leftIndex;
                if (leftIndex is IUnevaluated u_left) t_left = u_left.Evaluate(lookups);
                if (t_left != null && t_left is Number n_left)
                {
                    left = (int)n_left.value.Real;
                    if (!n_left.IsReal || left != n_left.value.Real)
                        return new Error("IndexError", "Indexer values must be positive integers.");
                    if (left < 0 || left >= item.Length)
                        return new Error("IndexError", $"{left} is out of range.");
                }
                IType t_right = rightIndex;
                if (rightIndex is IUnevaluated u_right) t_right = u_right.Evaluate(lookups);
                if (t_right != null &&  t_right is Number n_right)
                {
                    right = (int)n_right.value.Real;
                    if (!n_right.IsReal || right != n_right.value.Real)
                        return new Error("IndexError", "Indexer values must be positive integers.");
                    if (right < 0 || right >= item.Length)
                        return new Error("IndexError", $"{right} is out of range.");
                }

                if (range)
                {
                    if (left > right)
                        return new Error("IndexError", "The first index in an index range cannot be greater than the second index.");
                    return item.IndexRange(left, right);
                }
                else
                {
                    return item.Index(left);
                }
            }
            else
            {
                return new Error("TypeError", $"The type {expr.TypeName} cannot be indexed");
            }
        }
    }
}
