using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FAILang.Types;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FAILang.Types.Unevaluated;
using System.Numerics;

namespace FAILang
{
    public class FAILangVisitor : FAILangBaseVisitor<IType>
    {
        public new IType[] VisitCompileUnit([NotNull] FAILangParser.CompileUnitContext context)
        {
            return VisitCalls(context.calls());
        }

        public new IType[] VisitCalls([NotNull] FAILangParser.CallsContext context)
        {
            return context.call().Select(call => VisitCall(call)).ToArray();
        }

        public override IType VisitCall([NotNull] FAILangParser.CallContext context)
        {
            if (context.def() != null)
            {
                return VisitDef(context.def());
            }
            if (context.expression() != null)
                return VisitExpression(context.expression());
            return new Error("ParseError", "The input failed to parse.");
        }

        public override IType VisitDef([NotNull] FAILangParser.DefContext context)
        {
            var name = context.name().GetText();
            var update = context.update;
            var exp = context.expression();

            if (Global.Instance.reservedNames.Contains(name))
                return new Error("DefineFailed", $"{name} is a reserved name.");
            if (update == null && Global.Instance.globalVariables.ContainsKey(name))
                return new Error("DefineFailed", "The update keyword is required to change a function or variable.");

            bool memoize = context.memoize != null;

            // `update memo name` pattern
            if (exp == null && memoize)
            {
                if (!Global.Instance.globalVariables.ContainsKey(name))
                    return new Error("UpdateFailed", $"{name} is not defined");
                if (Global.Instance.globalVariables.TryGetValue(name, out var val) && val is Function func)
                {
                    if (func.memoize)
                        func.memos.Clear();
                    else
                        func.memoize = true;
                }
                else
                    return new Error("UpdateFailed", $"{name} is not a function");
            }
            else if (context.fparams() != null)
            {
                IType expr = VisitExpression(exp);
                Function f = new Function(context.fparams().param().Select(x => x.GetText()).ToArray(), expr, memoize: memoize, elipsis: context.fparams().elipsis != null);

                Global.Instance.globalVariables[name] = f;
            }
            else
            {
                var v = Global.Instance.Evaluate(VisitExpression(exp));
                if (v is Error)
                    return v;
                Global.Instance.globalVariables[name] = v;
            }

            return null;
        }

        public override IType VisitExpression([NotNull] FAILangParser.ExpressionContext context)
        {
            return VisitRelational(context.relational());
        }

        public override IType VisitRelational([NotNull] FAILangParser.RelationalContext context)
        {
            if (context.binary().Length == 1)
            {
                return VisitBinary(context.binary(0));
            }
            var binaryNodes = context.binary();
            var ops = context.relational_op();

            RelationalOperator[] opers = new RelationalOperator[ops.Length];
            for (int i = 0; i < opers.Length; i++)
            {
                switch (ops[i].GetText())
                {
                    case "=":
                        opers[i] = RelationalOperator.EQUALS;
                        break;
                    case "~=":
                        opers[i] = RelationalOperator.NOT_EQUALS;
                        break;
                    case ">":
                        opers[i] = RelationalOperator.GREATER;
                        break;
                    case "<":
                        opers[i] = RelationalOperator.LESS;
                        break;
                    case ">=":
                        opers[i] = RelationalOperator.GREATER_EQUAL;
                        break;
                    case "<=":
                        opers[i] = RelationalOperator.LESS_EQUAL;
                        break;
                }
            }
            return new RelationalOperatorExpression(opers, binaryNodes.Select(x => VisitBinary(x)).ToArray());
        }

        public override IType VisitBinary([NotNull] FAILangParser.BinaryContext context)
        {
            if (context.prefix() != null)
            {
                return VisitPrefix(context.prefix());
            }

            var binaryNodes = context.binary();
            BinaryOperator oper = BinaryOperator.MULTIPLY;
            switch (context.op.Text)
            {
                case "+":
                    oper = BinaryOperator.ADD;
                    break;
                case "-":
                    oper = BinaryOperator.SUBTRACT;
                    break;
                case "*":
                    oper = BinaryOperator.MULTIPLY;
                    break;
                case "":
                    oper = BinaryOperator.MULTIPLY;
                    break;
                case "/":
                    oper = BinaryOperator.DIVIDE;
                    break;
                case "%":
                    oper = BinaryOperator.MODULO;
                    break;
                case "^":
                    oper = BinaryOperator.EXPONENT;
                    break;
                case "is":
                    oper = BinaryOperator.IS;
                    break;
            }
            return new BinaryOperatorExpression(oper, VisitBinary(binaryNodes[0]), VisitBinary(binaryNodes[1]));
        }

        public override IType VisitPrefix([NotNull] FAILangParser.PrefixContext context)
        {
            if (context.op == null)
            {
                return VisitPostfix(context.postfix());
            }
            else
            {
                UnaryOperator prefix = UnaryOperator.NOT;
                switch (context.op.Text)
                {
                    case "~":
                        prefix = UnaryOperator.NOT;
                        break;
                    case "-":
                        prefix = UnaryOperator.NEGATIVE;
                        break;
                }
                return new UnaryOperatorExpression(prefix, VisitPostfix(context.postfix()));
            }
        }

        public override IType VisitPostfix([NotNull] FAILangParser.PostfixContext context)
        {
            if (context.indexer() == null)
            {
                return VisitAtom(context.atom());
            }
            else
            {
                var indexer = context.indexer();
                IType leftIndex = null;
                if (indexer.l_index != null)
                    leftIndex = VisitExpression(indexer.l_index);
                IType rightIndex = null;
                if (indexer.r_index != null)
                    rightIndex = VisitExpression(indexer.r_index);
                return new IndexerExpression(VisitAtom(context.atom()), leftIndex, rightIndex, indexer.elipsis != null);
            }
        }

        public override IType VisitAtom([NotNull] FAILangParser.AtomContext context)
        {
            if (context.expression() != null)
            {
                if (context.PIPE().Length == 2)
                    return new UnaryOperatorExpression(UnaryOperator.ABS, VisitExpression(context.expression()));
                return VisitExpression(context.expression());

            }
            else if (context.name() != null)
                return VisitName(context.name());
            else if (context.callparams() != null)    
            {
                return new FunctionExpression(
                    VisitAtom(context.atom()),
                    context.callparams().arg()
                        .Select(x => (VisitExpression(x.expression()), x.elipsis != null)).ToArray());
            }
            else if (context.union() != null)
                return VisitUnion(context.union());
            else if (context.lambda() != null)
                return VisitLambda(context.lambda());
            else if (context.piecewise() != null)
                return VisitPiecewise(context.piecewise());
            else if (context.t_number != null)
            {
                var number = context.t_number;
                if (number.Text.Equals("i"))
                    return new Number(Complex.ImaginaryOne);
                else if (number.Text.EndsWith('i'))
                    return new Number(Complex.ImaginaryOne * Convert.ToDouble(number.Text.TrimEnd('i')));
                else
                    return new Number(Convert.ToDouble(number.Text));
            }
            else if (context.t_string != null)
            {
                var str = context.t_string;
                string processed = str.Text.Substring(1, str.Text.Length - 2)
                    .Replace("\\\\", "\\")
                    .Replace("\\b", "\b")
                    .Replace("\\f", "\f")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\v", "\v")
                    .Replace("\\\"", "\"");
                return new MathString(processed);
            }
            else if (context.t_boolean != null)
                return context.t_boolean.Text.Equals("true") ? MathBool.TRUE : MathBool.FALSE;
            else if (context.t_undefined != null)
                return Types.Undefined.instance;
            else if (context.vector() != null)
                return VisitVector(context.vector());
            else if (context.tuple() != null)
                return VisitTuple(context.tuple());

            return null;
        }

        public override IType VisitName([NotNull] FAILangParser.NameContext context) =>
            new NamedArgument(context.GetText());

        public override IType VisitVector([NotNull] FAILangParser.VectorContext context) =>
            new UnevaluatedVector(context.expression().Select(x => VisitExpression(x)).ToArray());

        public override IType VisitTuple([NotNull] FAILangParser.TupleContext context) =>
            new UnevaluatedTuple(context.expression().Select(x => VisitExpression(x)).ToArray());

        public override IType VisitPiecewise([NotNull] FAILangParser.PiecewiseContext context)
        {
            var conditions = context.condition();
            IType[] conds = new IType[conditions.Length];
            IType[] exprs = new IType[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
            {
                conds[i] = VisitExpression(conditions[i].cond);
                exprs[i] = VisitExpression(conditions[i].expr);
            }
            return new CondExpression(conds, exprs, VisitExpression(context.expression()));
        }

        public override IType VisitLambda([NotNull] FAILangParser.LambdaContext context)
        {
            if (context.fparams() != null)
                return new Function(context.fparams().param().Select(x => x.GetText()).ToArray(), VisitExpression(context.expression()), memoize: context.memoize != null, elipsis: context.fparams().elipsis != null);
            else
                return new Function(new string[] { context.param().GetText() }, VisitExpression(context.expression()), memoize: false, elipsis: context.elipsis != null);
        }

        public override IType VisitUnion([NotNull] FAILangParser.UnionContext context)
        {
            var exprs = context.expression();
            IType[] vals = new IType[exprs.Length];
            for (int i = 0; i < exprs.Length; i++)
                vals[i] = VisitExpression(exprs[i]);
            return new Union(vals);
        }
    }
}
