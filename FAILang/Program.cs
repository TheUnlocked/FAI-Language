using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using FAILang.Builtins;
using FAILang.Tests;
using FAILang.Types;
using FAILang.Types.Unevaluated;

namespace FAILang
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.LoadBuiltins(new NumberBuiltinProvider());
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            foreach (ITest testPackage in new ITest[] { new LanguageTests() })
            {
                object firstIfOnly(IType[] list) => list.Length == 0 ? null : (list.Length == 1 ? list[0] : (object)list);
                foreach (var assertion in testPackage.Assertions)
                {
                    Debug.Assert(firstIfOnly(RunLines(assertion.Item1)).Equals(assertion.Item2));
                }
            }

            // Read-eval-print loop
            while (true) {
                string input = Console.ReadLine();
                while (input.EndsWith("  "))
                    input += Console.ReadLine();
                foreach (var val in RunLines(input))
                    if (val != null)
                        Console.WriteLine(val);
            }
        }

        public static IType[] RunLines(string input)
        {
            try
            {
                AntlrInputStream inputStream = new AntlrInputStream(input);

                FAILangLexer lexer = new FAILangLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
                FAILangParser parser = new FAILangParser(commonTokenStream);
                parser.ErrorHandler = new BailErrorStrategy();

                FAILangParser.CallsContext expressionContext = parser.calls();
                FAILangVisitor visitor = new FAILangVisitor();

                return visitor.VisitCalls(expressionContext).Select(x => Global.Evaluate(x)).ToArray();
            }
            catch (Antlr4.Runtime.Misc.ParseCanceledException)
            {
                Console.WriteLine(new Error("ParseError", "The input failed to parse."));
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