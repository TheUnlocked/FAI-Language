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
using FAILang.Grammar;
using System.IO;
using FAILang.Types.Unevaluated.Passthrough;

namespace FAILang
{
    public class FAILangVisitor : FAILangBaseVisitor<IType>
    {
        Global globalEnvironment;

        public FAILangVisitor(Global globalEnvironment)
        {
            this.globalEnvironment = globalEnvironment;
        }

        public new IType[] VisitCompileUnit([NotNull] FAILangParser.CompileUnitContext context)
        {
            return VisitEarlyCalls(context.earlyCalls()).Concat(VisitCalls(context.calls())).ToArray();
        }

        public new IType[] VisitEarlyCalls([NotNull] FAILangParser.EarlyCallsContext context)
        {
            List<IType> ret = new List<IType>();
            ret.AddRange(context.importStatement()?.Select(x => VisitImportStatement(x)));
            ret.AddRange(context.usingStatement()?.Select(x => VisitUsingStatement(x)));
            if (context.namespaceStatement() != null)
                ret.Add(VisitNamespaceStatement(context.namespaceStatement()));

            return ret.ToArray();

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

        static List<string> imported = new List<string>();
        public override IType VisitImportStatement([NotNull] FAILangParser.ImportStatementContext context)
        {
            IType Match(string filepath)
            {
                var importers = FAI.Instance.importers.Where(x => x.FileExtensions.Contains(Path.GetExtension(filepath))).ToArray();

                bool errored = false;
                foreach (var importer in importers)
                {
                    try
                    {
                        if (importer.TryImport(filepath, globalEnvironment))
                        {
                            return null;
                        }
                    }
                    catch(Exception)
                    {
                        errored = true;
                    }
                }
                if (errored)
                {
                    return new Error("ImportError", $"\"{filepath}\" failed to import.");
                }
                return new Error("ImportError", $"\"{filepath}\" is not a library file.");
            }

            string str = context.STRING().GetText().Trim('"');
            if (imported.Contains(str))
            {
                return null;
            }
            else
            {
                imported.Add(str);
            }
            if (Path.HasExtension(str)) {
                if (File.Exists(str))
                {
                    return Match(str);
                }
                else
                {
                    return new Error("ImportError", $"No library file \"{str}\" exists.");
                }
            }
            else
            {
                string dir = Path.GetDirectoryName(str),
                    name = Path.GetFileNameWithoutExtension(str);
                string[] validFiles;
                try
                {
                    validFiles = Directory.GetFiles(dir == "" ? Directory.GetCurrentDirectory() : dir, name + ".*")
                        .Where(x => FAI.Instance.importers
                            .Where(y => y.FileExtensions.Contains(Path.GetExtension(x))).Count() > 0)
                        .ToArray();
                }
                catch(Exception)
                {
                    return new Error("ImportError", $"\"{str}\" is not a valid import target.");
                }
                switch (validFiles.Length)
                {
                    case 0:
                        return new Error("ImportError", $"No library file \"{str}\" exists.");
                    case 1:
                        return Match(validFiles[0]);
                    default:
                        return new Error("ImportError", $"Multiple files match \"{str}\": {String.Join(", ", validFiles)}");
                }
            }
        }

        public override IType VisitUsingStatement([NotNull] FAILangParser.UsingStatementContext context)
        {
            if (context.name() != null)
            {
                var ns = Namespace.Root.GetSubNamespace(context.@namespace().NAME().Select(x => x.GetText()));
                var newDict = new Dictionary<string, IType>();
                foreach (var key in ns.Variables.Keys)
                {
                    newDict[context.name().GetText() + key] = ns.Variables[key];
                }
                return globalEnvironment.GlobalScope.AddParent(new Scope(null, newDict));
            }
            return globalEnvironment.GlobalScope.AddParent(
                Namespace.Root.GetSubNamespace(context.@namespace().NAME().Select(x => x.GetText())));
        }

        public override IType VisitNamespaceStatement([NotNull] FAILangParser.NamespaceStatementContext context)
        {
            globalEnvironment.Namespace = Namespace.Root.GetSubNamespace(context.@namespace().NAME().Select(x => x.GetText()));
            globalEnvironment.GlobalScope = new MultiScope(
                globalEnvironment.GlobalScope.bonusParents.Prepend(globalEnvironment.Namespace).ToArray(),
                globalEnvironment._globalVariables);
            return null;
        }

        public override IType VisitDef([NotNull] FAILangParser.DefContext context)
        {
            var name = context.name().GetText();
            var update = context.update;
            var exp = context.expression();

            if (globalEnvironment.reservedNames.Contains(name))
                return new Error("DefineFailed", $"{name} is a reserved name.");
            if (update == null && globalEnvironment.GlobalScope.HasVar(name))
                return new Error("DefineFailed", "The update keyword is required to change a function or variable.");

            bool memoize = context.memoize != null;

            // `update memo name` pattern
            if (exp == null && memoize)
            {
                if (!globalEnvironment.GlobalScope.HasVar(name))
                    return new Error("UpdateFailed", $"{name} is not defined");
                if (globalEnvironment.GlobalScope.TryGetVar(name, out var val) && val is Function func)
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
                Function f = new Function(context.fparams().param().Select(x => x.GetText()).ToArray(), expr, globalEnvironment.GlobalScope, memoize: memoize, elipsis: context.fparams().elipsis != null);

                if (globalEnvironment.Namespace == Namespace.Root)
                {
                    globalEnvironment._globalVariables[name] = f;
                }
                else
                {
                    globalEnvironment.Namespace.Variables[name] = f;
                }
            }
            else
            {
                var v = globalEnvironment.Evaluate(VisitExpression(exp));
                if (v is Error)
                    return v;
                if (globalEnvironment.Namespace == Namespace.Root)
                {
                    globalEnvironment._globalVariables[name] = v;
                }
                else
                {
                    globalEnvironment.Namespace.Variables[name] = v;
                }
            }

            return null;
        }

        public override IType VisitExpression([NotNull] FAILangParser.ExpressionContext context)
        {
            return VisitWhere(context.where());
        }

        public override IType VisitWhere([NotNull] FAILangParser.WhereContext context)
        {
            //if (context.WHERE() != null)
            //{
            //    return new FunctionExpression(
            //        new UnevaluatedFunction(
            //            context.name().Select(name => name.GetText()).Append("self").ToArray(),
            //            VisitBoolean(context.boolean())),
            //        context.expression()
            //            .Select(expr => (VisitExpression(expr), false))
            //            .Append((new NamedArgument("self"), false))
            //            .ToArray());
            //}
            if (context.WHERE() != null)
            {
                return new WhereExpression(VisitBoolean(context.boolean()),
                    new Dictionary<string, IType>(context.name().Zip(context.expression(),
                        (name, expr) => new KeyValuePair<string, IType>(name.GetText(), VisitExpression(expr)))));
            }
            return VisitBoolean(context.boolean());
        }

        public override IType VisitBoolean([NotNull] FAILangParser.BooleanContext context)
        {
            if (context.op != null)
            {
                var relationalNodes = context.relational();
                BinaryOperator oper = BinaryOperator.MULTIPLY;
                switch (context.op.Text)
                {
                    case "and":
                        oper = BinaryOperator.AND;
                        break;
                    case "or":
                        oper = BinaryOperator.OR;
                        break;
                }
                return new BinaryOperatorExpression(oper, VisitRelational(relationalNodes[0]), VisitRelational(relationalNodes[1]));
            }
            return VisitRelational(context.relational(0));
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
                case "+-":
                    oper = BinaryOperator.PLUS_MINUS;
                    break;
                case "*":
                    oper = BinaryOperator.MULTIPLY;
                    break;
                case "/":
                    oper = BinaryOperator.DIVIDE;
                    break;
                case "^":
                    oper = BinaryOperator.EXPONENT;

                    if (binaryNodes[0].prefix() != null)
                    {
                        // Exponentation is an edge-case in which the prefix operator needs to go after it. e.g. -2^2 = -4; -2x^2 where x=5 = -50
                        UnaryOperator? prefix = null;
                        Number? multiplier = null;
                        IType atom = binaryNodes[0].prefix().multiplier().atom()?.Pipe(VisitAtom);

                        if (binaryNodes[0].prefix().op != null)
                        {
                            switch (binaryNodes[0].prefix().op.Text)
                            {
                                case "~":
                                    prefix = UnaryOperator.NOT;
                                    break;
                                case "-":
                                    prefix = UnaryOperator.NEGATIVE;
                                    break;
                                case "+-":
                                    prefix = UnaryOperator.PLUS_MINUS;
                                    break;
                            }
                        }
                        if (binaryNodes[0].prefix().multiplier().NUMBER() != null)
                        {
                            multiplier = new Number(Convert.ToDouble(binaryNodes[0].prefix().multiplier().t_number.Text));
                            if (atom == null)
                            {
                                atom = multiplier;
                                multiplier = null;
                            }
                        }
                        if (prefix != null)
                        {
                            if (multiplier != null)
                            {
                                return new UnaryOperatorExpression(prefix.Value,
                                    new BinaryOperatorExpression(BinaryOperator.MULTIPLY, multiplier,
                                        new BinaryOperatorExpression(oper, atom,
                                            VisitBinary(binaryNodes[1]))));
                            }
                            else
                            {
                                return new UnaryOperatorExpression(prefix.Value,
                                    new BinaryOperatorExpression(oper, atom,
                                        VisitBinary(binaryNodes[1])));
                            }

                        }
                        else
                        {
                            if (multiplier != null)
                            {
                                return new BinaryOperatorExpression(BinaryOperator.MULTIPLY, multiplier,
                                    new BinaryOperatorExpression(oper, atom,
                                        VisitBinary(binaryNodes[1])));
                            }
                            else
                            {
                                return new BinaryOperatorExpression(oper, atom,
                                    VisitBinary(binaryNodes[1]));
                            }
                        }

                    }
                    break;
                case "||":
                    oper = BinaryOperator.CONCAT;
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
                return VisitMultiplier(context.multiplier());
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
                    case "+-":
                        prefix = UnaryOperator.PLUS_MINUS;
                        break;
                }
                return new UnaryOperatorExpression(prefix, VisitMultiplier(context.multiplier()));
            }
        }

        public override IType VisitMultiplier([NotNull] FAILangParser.MultiplierContext context)
        {
            if (context.t_number != null)
            {
                var number = context.t_number;
                Number n;
                //if (number.Text.Equals("i"))
                //    n = new Number(Complex.ImaginaryOne);
                //else if (number.Text.EndsWith('i'))
                //    n = new Number(Complex.ImaginaryOne * Convert.ToDouble(number.Text.TrimEnd('i')));
                //else
                n = new Number(Convert.ToDouble(number.Text));

                if (context.atom() != null)
                {
                    return new BinaryOperatorExpression(BinaryOperator.MULTIPLY, n, VisitAtom(context.atom()));
                }
                else
                    return n;
            }
            else
                return VisitAtom(context.atom());
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
            else if (context.indexer() != null)
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
            else if (context.union() != null)
                return VisitUnion(context.union());
            else if (context.lambda() != null)
                return VisitLambda(context.lambda());
            else if (context.piecewise() != null)
                return VisitPiecewise(context.piecewise());
            else if (context.vector() != null)
                return VisitVector(context.vector());
            //else if (context.map() != null)
            //    return VisitMap(context.map());
            else if (context.tuple() != null)
                return VisitTuple(context.tuple());
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
                return Undefined.Instance;

            return null;
        }

        public override IType VisitName([NotNull] FAILangParser.NameContext context) =>
            new NamedArgument(context.NAME().GetText(), globalEnvironment);

        public override IType VisitVector([NotNull] FAILangParser.VectorContext context) =>
            new UnevaluatedVector(context.expression().Select(x => VisitExpression(x)).ToArray());

        //public override IType VisitMap([NotNull] FAILangParser.MapContext context) =>
        //    new UnevaluatedMap(
        //        context.expression().Where((v, i) => i % 2 == 0).Select(x => VisitExpression(x)).ToArray(),
        //        context.expression().Where((v, i) => i % 2 == 1).Select(x => VisitExpression(x)).ToArray());

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
            IType otherwise = Undefined.Instance;
            if (context.OTHERWISE() != null)
                otherwise = VisitExpression(context.expression());

            return new CondExpression(conds, exprs, otherwise);
        }

        public override IType VisitLambda([NotNull] FAILangParser.LambdaContext context)
        {
            if (context.fparams() != null)
                return new UnevaluatedFunction(context.fparams().param().Select(x => x.GetText()).ToArray(), VisitExpression(context.expression()), memoize: context.memoize != null, elipsis: context.fparams().elipsis != null);
            else
                return new UnevaluatedFunction(new string[] { context.param().GetText() }, VisitExpression(context.expression()), memoize: false, elipsis: context.elipsis != null);
        }

        public override IType VisitUnion([NotNull] FAILangParser.UnionContext context)
        {
            var exprs = context.expression();
            IType[] vals = new IType[exprs.Length];
            for (int i = 0; i < exprs.Length; i++)
                vals[i] = VisitExpression(exprs[i]);
            return new UnevaluatedUnion(vals);
        }
    }
}
