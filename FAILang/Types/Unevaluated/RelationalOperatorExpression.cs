using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FAILang.Types.Unevaluated.Passthrough;

namespace FAILang.Types.Unevaluated
{
    class RelationalOperatorExpression : IUnevaluated
    {
        public string TypeName => "OperatorExpression";

        public RelationalOperator[] ops;
        public IType[] ins;

        public RelationalOperatorExpression(RelationalOperator[] ops, IType[] ins)
        {
            this.ops = ops;
            this.ins = ins;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            var first = ins[0];
            var second = ins[1];
            if (first is IUnevaluated uFirst)
                first = uFirst.Evaluate(lookups);
            if (second is IUnevaluated uSecond)
                second = uSecond.Evaluate(lookups);
            if (first is Union unionFirst)
            {
                IType[] result = new IType[unionFirst.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    var newIns = ins;
                    newIns[0] = first;
                    result[i] = new RelationalOperatorExpression(ops, newIns).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (second is Union unionSecond)
            {
                IType[] result = new IType[unionSecond.values.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    var newIns = ins;
                    newIns[1] = second;
                    result[i] = new RelationalOperatorExpression(ops, newIns).Evaluate(lookups);
                }
                return new Union(result, lookups);
            }
            if (first is IUnevaluated || second is IUnevaluated) {
                var newIns = ins.ToArray();
                newIns[0] = first;
                newIns[1] = second;
                return new BakedExpression(new RelationalOperatorExpression(ops, newIns), lookups);
            }

            if (first is Error)
                return first;
            if (second is Error)
                return second;

            if (first == Undefined.Instance || second == Undefined.Instance)
                return Undefined.Instance;

            // Operate
            if (first is IOperable firstOp && second is IOperable secondOp)
            {
                if (firstOp.RelativeOperators != null && secondOp.RelativeOperators != null && firstOp.RelativeOperators.TryGetValue(ops[0], out var firstAction) && secondOp.RelativeOperators.ContainsKey(ops[0]))
                {
                    MathBool ret = firstAction.Invoke(secondOp);
                    if (ret == null)
                        return new Error("WrongType", $"The {ops[0].ToDisplayString()} operator cannot be applied to types {first.TypeName} and {second.TypeName}");
                    if (ret == MathBool.TRUE)
                    {
                        if (ops.Length == 1)
                            return ret;
                        var newIns = ins.Skip(1).ToArray();
                        newIns[0] = second;
                        return new RelationalOperatorExpression(ops.Skip(1).ToArray(), newIns);
                    }
                    return ret;
                }
            }
            return new Error("WrongType", $"The {ops[0].ToDisplayString()} operator cannot be applied to types {first.TypeName} and {second.TypeName}");
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + EqualityComparer<RelationalOperator[]>.Default.GetHashCode(ops);
            hash = hash * 1532528149 + EqualityComparer<IType[]>.Default.GetHashCode(ins);
            return hash;
        }
    }
}
