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
        public override IType VisitCalls([NotNull] FAILangParser.CallsContext context)
        {
            foreach (var call in context.call()) VisitCall(call);
            return null;
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
            var fname = context.fname()?.GetText();
            var vname = context.vname()?.GetText();
            var update = context.update;
            var exp = context.expression();

            if (fname != null)
            {
                if (Global.reservedNames.Contains(fname))
                    return new Error("DefineFailed", $"{fname} is a reserved name.");
                if (update == null && (Global.functions.ContainsKey(fname) || Global.variables.ContainsKey(fname)))
                {
                    return new Error("DefineFailed", "The update keyword is required to change the definition of a function or outer argument.");
                }

                bool memoize = context.memoize != null;

                if (Global.functions.ContainsKey(fname) && exp == null && memoize)
                {
                    var f = Global.functions[fname];
                    if (f.memoize)
                        f.memos.Clear();
                    else
                        return new Error("UpdateFailed", $"{fname} is not memoized");
                }
                else
                {
                    IType expr = VisitExpression(exp);
                    Function f = new Function(context.fparams().arg().Select(x => x.GetText()).ToArray(), expr, memoize);

                    Global.variables.Remove(fname);
                    Global.functions[fname] = f;
                }
            }
            else if (vname != null)
            {
                if (Global.reservedNames.Contains(vname))
                    return new Error("DefineFailed", $"{vname} is a reserved name.");
                if (update == null && (Global.functions.ContainsKey(vname) || Global.variables.ContainsKey(vname)))
                {
                    return new Error("DefineFailed", "The update keyword is required to change the definition of a function or outer argument.");
                }
                Global.functions.Remove(vname);
                var v = Global.Evaluate(VisitExpression(exp));
                if (v is Error)
                    return v;
                Global.variables[vname] = v;
            }
            return null;
        }

        public override IType VisitExpression([NotNull] FAILangParser.ExpressionContext context)
        {
            var type = context.type();
            var arg = context.arg();
            var op = context.op;
            var prefix = context.prefix();
            var cond = context.cond();
            var callparams = context.callparams();
            var lambda = context.lambda();
            var union = context.union();
            var indexer = context.indexer();

            if      (type != null)      return VisitType(type);
            else if (arg != null)       return VisitArg(arg);
            else if (op != null)
            {
                var exprs = context.expression();
                Operator oper = Operator.MULTIPLY;
                switch (op.Text)
                {
                    case "+":
                        oper = Operator.ADD;
                        break;
                    case "-":
                        oper = Operator.SUBTRACT;
                        break;
                    case "*":
                        oper = Operator.MULTIPLY;
                        break;
                    case "":
                        oper = Operator.MULTIPLY;
                        break;
                    case "/":
                        oper = Operator.DIVIDE;
                        break;
                    case "%":
                        oper = Operator.MODULO;
                        break;
                    case "^":
                        oper = Operator.EXPONENT;
                        break;
                    case "=":
                        oper = Operator.EQUALS;
                        break;
                    case "~=":
                        oper = Operator.NOT_EQUALS;
                        break;
                    case ">":
                        oper = Operator.GREATER;
                        break;
                    case "<":
                        oper = Operator.LESS;
                        break;
                    case ">=":
                        oper = Operator.GR_EQUAL;
                        break;
                    case "<=":
                        oper = Operator.LE_EQUAL;
                        break;
                }
                return new OperatorExpression(oper, VisitExpression(exprs[0]), VisitExpression(exprs[1]));
            }
            else if (prefix != null)    
            {
                Prefix pre = Prefix.NOT;
                switch (prefix.GetText())
                {
                    case "~":
                        pre = Prefix.NOT;
                        break;
                    case "-":
                        pre = Prefix.NEGATIVE;
                        break;
                }
                return new PrefixExpression(pre, VisitExpression(context.expression(0)));
            }
            else if (cond != null)      return VisitCond(cond);
            else if (callparams != null)    
            {
                var exprs = callparams.expression();
                IType[] ins = new IType[exprs.Length];
                for (int i = 0; i < ins.Length; i++)
                {
                    ins[i] = VisitExpression(exprs[i]);
                }

                return new FunctionExpression(VisitExpression(context.expression(0)), ins);
            }
            else if (lambda != null)    return VisitLambda(lambda);
            else if (union != null)     return VisitUnion(union);
            else if (indexer != null)   
            {
                IType l_expr = null;
                if (indexer.l_index != null)
                    l_expr = VisitExpression(indexer.l_index);
                IType r_expr = null;
                if (indexer.r_index != null)
                    r_expr = VisitExpression(indexer.r_index);
                return new IndexerExpression(VisitExpression(context.expression(0)), l_expr, r_expr, indexer.elipsis != null);
            }

            return VisitExpression(context.expression(0));
        }

        public override IType VisitArg([NotNull] FAILangParser.ArgContext context) =>
            new NamedArgument(context.GetText());

        public override IType VisitType([NotNull] FAILangParser.TypeContext context)
        {
            var number = context.t_number;
            var str = context.t_string;
            var boolean = context.t_boolean;
            var void_type = context.t_void;
            var vector = context.vector();

            if (number != null)
            {
                if (number.Text.Equals("i"))
                    return new Number(Complex.ImaginaryOne);
                else if (number.Text.EndsWith('i'))
                    return new Number(Complex.ImaginaryOne * Convert.ToDouble(number.Text.TrimEnd('i')));
                else
                    return new Number(Convert.ToDouble(number.Text));
            }
            else if (str != null)
            {
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
            else if (boolean != null)
                return boolean.Text.Equals("true") ? MathBool.TRUE : MathBool.FALSE;
            else if (vector != null)
                return VisitVector(vector);
            else
                return Types.Void.instance;
        }

        public override IType VisitVector([NotNull] FAILangParser.VectorContext context) =>
            new UnevaluatedVector(context.expression().Select(x => VisitExpression(x)).ToArray());

        public override IType VisitCond([NotNull] FAILangParser.CondContext context)
        {
            var conditions = context.condition();
            IType[] conds = new IType[conditions.Length];
            IType[] exprs = new IType[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
            {
                conds[i] = VisitExpression(conditions[i].expression(0));
                exprs[i] = VisitExpression(conditions[i].expression(1));
            }
            return new CondExpression(conds, exprs, VisitExpression(context.expression()));
        }

        public override IType VisitLambda([NotNull] FAILangParser.LambdaContext context) =>
            new Function(context.fparams().arg().Select(x => x.GetText()).ToArray(), VisitExpression(context.expression()), context.memoize != null);

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
