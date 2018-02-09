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
                    Expression expr = VisitExpression(exp) as Expression;
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
                Global.variables[vname] = VisitExpression(exp) as Expression;
            }
            return null;
        }

        public override IType VisitExpression([NotNull] FAILangParser.ExpressionContext context)
        {
            var type = context.type();
            var arg = context.arg();
            var op = context.op;
            bool sideMultiply = context.expression().Length == 2 && op == null;
            var prefix = context.prefix();
            var cond = context.cond();
            var callparams = context.callparams();
            var lambda = context.lambda();
            var union = context.union();

            IType result = null;

            if (type != null)
            {
                var number = type.t_number;
                var str = type.t_string;
                var boolean = type.t_boolean;
                var void_type = type.t_void;

                if (number != null)
                {
                    if (number.Text.EndsWith('i'))
                        result = new Number(Complex.ImaginaryOne * Convert.ToDouble(number.Text.TrimEnd('i')));
                    else
                        result = new Number(Convert.ToDouble(number.Text));
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
                    result = new MathString(processed);
                }
                else if (boolean != null)
                {
                    result = boolean.Text.Equals("true") ? MathBool.TRUE : MathBool.FALSE;
                }
                else if (void_type != null) {
                    result = Types.Void.instance;
                }
            }
            else if (arg != null)
            {
                return new Expression(new NamedArgument(arg.GetText()));
            }
            else if (op != null || sideMultiply)
            {
                var exprs = context.expression();
                Operator oper = Operator.MULTIPLY;
                if (!sideMultiply)
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
                result = new OperatorExpression(oper, VisitExpression(exprs[0]) as Expression, VisitExpression(exprs[1]) as Expression);
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
                result = new PrefixExpression(pre, VisitExpression(context.expression(0)) as Expression);
            }
            else if (cond != null)
            {
                var conditions = cond.condition();
                Expression[] conds = new Expression[conditions.Length];
                Expression[] exprs = new Expression[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                {
                    conds[i] = VisitExpression(conditions[i].expression(0)) as Expression;
                    exprs[i] = VisitExpression(conditions[i].expression(1)) as Expression;
                }
                result = new CondExpression(conds, exprs, VisitExpression(cond.expression()) as Expression);
            }
            else if (callparams != null)
            {
                var exprs = callparams.expression();
                IType[] ins = new IType[exprs.Length];
                for (int i = 0; i < ins.Length; i++)
                {
                    ins[i] = VisitExpression(exprs[i]);
                }

                result = new FunctionExpression(VisitExpression(context.expression(0)) as Expression, ins);
            }
            else if (lambda != null)
            {
                Expression expr = VisitExpression(lambda.expression()) as Expression;
                result = new Function(lambda.fparams().arg().Select(x => x.GetText()).ToArray(), expr);
            }
            else if (union != null)
            {
                var exprs = union.expression();
                IType[] vals = new IType[exprs.Length];
                for (int i = 0; i < exprs.Length; i++)
                    vals[i] = VisitExpression(exprs[i]);
                result = new Union(vals);
            }
            else
            {
                return VisitExpression(context.expression(0)) as Expression;
            }

            return new Expression(result);
        }
    }
}
