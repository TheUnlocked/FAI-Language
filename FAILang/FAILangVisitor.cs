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
            if (context.defStatement() != null)
            {
                return VisitDefStatement(context.defStatement());
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

        public override IType VisitDefStatement([NotNull] FAILangParser.DefStatementContext context)
        {
            var update = context.update;
            var def = context.def();
            var updateMemoName = context.name()?.GetText();
            var defName = def.name()?.GetText();

            if (updateMemoName != null)
            {
                // `update memo name` pattern
                if (!globalEnvironment.GlobalScope.HasVar(updateMemoName))
                    return new Error("UpdateFailed", $"{updateMemoName} is not defined");
                if (globalEnvironment.GlobalScope.TryGetVar(updateMemoName, out var val) && val is Function func)
                {
                    if (func.memoize)
                        func.memos.Clear();
                    else
                        func.memoize = true;
                }
                else
                {
                    return new Error("UpdateFailed", $"{updateMemoName} is not a function");
                }
            }

            if (globalEnvironment.reservedNames.Contains(defName))
                return new Error("DefineFailed", $"{defName} is a reserved name.");
            if (update == null && globalEnvironment.GlobalScope.HasVar(defName))
                return new Error("DefineFailed", "The update keyword is required to change a function or variable.");

            var (_, value) = VisitDef(def);

            while (value is IUnevaluated u)
            {
                value = u.Evaluate(globalEnvironment.GlobalScope);
            }

            if (value is Error)
            {
                return value;
            }

            if (globalEnvironment.Namespace == Namespace.Root)
            {
                globalEnvironment._globalVariables[defName] = value;
            }
            else
            {
                globalEnvironment.Namespace.Variables[defName] = value;
            }
            return null;
        }

        public new (string name, IType value) VisitDef([NotNull] FAILangParser.DefContext context)
        {
            var name = context.name().GetText();
            var exp = context.expression();

            bool memoize = context.memoize != null;

            if (context.fparams() != null)
            {
                IType expr = VisitExpression(exp);
                var f = new UnevaluatedFunction(context.fparams().param().Select(x => x.GetText()).ToArray(), expr, memoize: memoize, elipsis: context.fparams().elipsis != null);

                return (name, f);
            }
            else
            {
                var v = VisitExpression(exp);
                if (v is Error)
                    return (null, v);
                return (name, v);
            }
        }

        public override IType VisitExpression([NotNull] FAILangParser.ExpressionContext context)
        {
            return VisitWhere(context.where());
        }

        public override IType VisitWhere([NotNull] FAILangParser.WhereContext context)
        {
            if (context.WHERE() != null)
            {
                var defPairs = context.def().Select(ctx => VisitDef(ctx));
                var errors = defPairs.Where(x => x.value is Error);
                if (errors.Count() > 0)
                {
                    return errors.First().value;
                }
                return new WhereExpression(VisitBoolean(context.boolean()),
                    new Dictionary<string, IType>(defPairs.Select(t => new KeyValuePair<string, IType>(t.name, t.value))));
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

                if (context.exponent() != null)
                {
                    return new BinaryOperatorExpression(BinaryOperator.MULTIPLY, n, VisitExponent(context.exponent()));
                }
                else
                    return n;
            }
            else
                return VisitExponent(context.exponent());
        }

        public override IType VisitExponent([NotNull] FAILangParser.ExponentContext context)
        {
            if (context.EXPONENT() != null)
            {
                return new BinaryOperatorExpression(BinaryOperator.EXPONENT, VisitAtom(context.atom()), VisitPrefix(context.prefix()));
            }
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
            else if (context.NUMBER() != null)
                return new Number(Convert.ToDouble(context.NUMBER().GetText()));
            else if (context.STRING() != null)
            {
                var str = context.STRING().GetText();
                string processed = str[1..^1]
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
            else if (context.BOOLEAN() != null)
                return context.BOOLEAN().GetText().Equals("true") ? MathBool.TRUE : MathBool.FALSE;
            else if (context.UNDEFINED() != null)
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
