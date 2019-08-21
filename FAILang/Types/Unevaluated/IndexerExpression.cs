using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FAILang.Types.Unevaluated.Passthrough;

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

        public IType Evaluate(Scope scope)
        {
            IType expr = expression;
            if (expr is IUnevaluated u)
            {
                expr = u.Evaluate(scope);
            }
            if (expr is Union union)
            {
                return new Union(union.values.Select(x => new IndexerExpression(x, leftIndex, rightIndex, range).Evaluate(scope)).ToArray()).Evaluate(scope);
            }
            if (expr is IUnevaluated && !(expr is Union))
            {
                return new BakedExpression(new IndexerExpression(expr, leftIndex, rightIndex, range), scope);
            }
            else if (expr is IIndexable item)
            {
                int left = 0;
                int right = item.Length - 1;

                IType t_left = leftIndex;
                if (leftIndex is IUnevaluated u_left) t_left = u_left.Evaluate(scope);
                if (t_left is Union un_left)
                {
                    return new Union(un_left.values.Select(x => new IndexerExpression(expr, x, rightIndex, range).Evaluate(scope)).ToArray()).Evaluate(scope);
                }
                if (t_left is IUnevaluated)
                {
                    return new BakedExpression(new IndexerExpression(expr, t_left, rightIndex, range), scope);
                }
                if (t_left != null && t_left is Number n_left)
                {
                    left = (int)n_left.value.Real;
                    if (!n_left.IsReal || left != n_left.value.Real)
                        return new Error("IndexError", "Indexer values must be positive integers.");
                    if (!range && (left < 0 || left >= item.Length))
                        return Undefined.Instance;
                }
                else if (t_left != null)
                {
                    return Undefined.Instance;
                }

                IType t_right = rightIndex;
                if (rightIndex is IUnevaluated u_right) t_right = u_right.Evaluate(scope);
                if (t_left is Union un_right)
                {
                    return new Union(un_right.values.Select(x => new IndexerExpression(expr,t_left, x, range).Evaluate(scope)).ToArray()).Evaluate(scope);
                }
                if (t_right is IUnevaluated)
                {
                    return new BakedExpression(new IndexerExpression(expr, t_left, t_right, range), scope);
                }
                if (t_right != null && t_right is Number n_right)
                {
                    right = (int)n_right.value.Real;
                    if (!n_right.IsReal || right != n_right.value.Real)
                        return new Error("IndexError", "Indexer values must be positive integers.");
                    if (!range && (right < 0 || right >= item.Length))
                        return Undefined.Instance;
                }
                else if (t_right != null)
                {
                    return Undefined.Instance;
                }

                if (range)
                {
                    if (t_right == null && right < left)
                        right = left;
                    else if (t_left == null && right < left)
                        left = right;
                    if (left > right)
                    {
                        var tmp = left;
                        left = right;
                        right = tmp;
                    }
                        
                    return item.IndexRange(left, right);
                }
                else
                {
                    return item.Index(left);
                }
            }
            else
            {
                if (expr is Error || expr is Undefined)
                    return expr;
                return new Error("TypeError", $"The type {expr.TypeName} cannot be indexed");
            }
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + expression.GetHashCode();
            hash = hash * 1532528149 + leftIndex.GetHashCode();
            hash = hash * 1532528149 + rightIndex.GetHashCode();
            hash = hash * 1532528149 + range.GetHashCode();
            return hash;
        }
    }
}
