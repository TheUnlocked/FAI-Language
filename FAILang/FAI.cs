using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using FAILang.Builtins;
using FAILang.Grammar;
using FAILang.Importers;
using FAILang.Types;

namespace FAILang
{
    public class FAI
    {
        private static FAI _instance;
        public static FAI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FAI();
                return _instance;
            }
        }

        public static void ClearInstance() => _instance = null;

        private FAI()
        {
        }

        public readonly List<IImporter> importers = new List<IImporter>();
        public readonly List<IBuiltinProvider> builtinProviders = new List<IBuiltinProvider>();

        public void LoadImporters(params IImporter[] importers)
        {
            this.importers.AddRange(importers);
        }

        public void ProvideBuiltins(params IBuiltinProvider[] builtinProviders)
        {
            this.builtinProviders.AddRange(builtinProviders);
        }

        public IType[] InterpretLines(Global environment, string input)
        {
            if (input == null)
                return new IType[] { };
            try
            {
                AntlrInputStream inputStream = new AntlrInputStream(input);

                FAILangLexer lexer = new FAILangLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
                FAILangParser parser = new FAILangParser(commonTokenStream);
                parser.ErrorHandler = new BailErrorStrategy();

                FAILangParser.CompileUnitContext expressionContext = parser.compileUnit();
                FAILangVisitor visitor = new FAILangVisitor(environment);

                return visitor.VisitCompileUnit(expressionContext).Select(x => environment.Evaluate(x)).ToArray();
            }
            catch (Antlr4.Runtime.Misc.ParseCanceledException)
            {
                return new IType[] { new Error("ParseError", "The input failed to parse.") };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return new IType[] { };
        }
    }

}