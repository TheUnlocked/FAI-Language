using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FAILang.Types.Unevaluated.Passthrough;
using System.Collections;

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

        public IType Evaluate(Scope scope)
        {
            var first = ins[0];
            var second = ins[1];
            if (first is IUnevaluated uFirst)
                first = uFirst.Evaluate(scope);
            if (second is IUnevaluated uSecond)
                second = uSecond.Evaluate(scope);
            if (first is Union unionFirst)
            {
                return unionFirst.Apply(x => new RelationalOperatorExpression(ops, new IType[] { x }.Concat(ins[1..]).ToArray()));
            }
            if (second is Union unionSecond)
            {
                return unionSecond.Apply(x => new RelationalOperatorExpression(ops, new IType[] { ins[0], x }.Concat(ins[2..]).ToArray()));
            }
            if (first is IUnevaluated || second is IUnevaluated) {
                var newIns = ins.ToArray();
                newIns[0] = first;
                newIns[1] = second;
                return new BakedExpression(new RelationalOperatorExpression(ops, newIns), scope);
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
                        var newIns = ins[1..];
                        newIns[0] = second;
                        return new RelationalOperatorExpression(ops[1..], newIns);
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
            hash = hash * 1532528149 + ((IStructuralEquatable)ins).GetHashCode(EqualityComparer<IType>.Default);
            return hash;
        }
    }
}
