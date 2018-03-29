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
    public class FAI
    {
        public FAI()
        {
            Global.ResetGlobals();
        }

        static void Main(string[] args)
        {
            FAI fai = new FAI();

            Global.Instance.LoadBuiltins(new NumberBuiltinProvider(), new CollectionBuiltinProvider());
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            foreach (ITest testPackage in new ITest[] { new LanguageTests() })
            {
                object firstIfOnly(IType[] list) => list.Length == 0 ? null : (list.Length == 1 ? list[0] : (object)list);
                foreach (var assertion in testPackage.Assertions)
                {
                    Debug.Assert(firstIfOnly(fai.RunLines(assertion.Item1)).Equals(assertion.Item2));
                }
            }

            // Read-eval-print loop
            while (true) {
                string input = Console.ReadLine();
                while (input.EndsWith("  "))
                    input += Console.ReadLine();
                
                foreach (var val in fai.RunLines(input))
                    if (val != null)
                        Console.WriteLine(val);
            }
        }

        public IType[] RunLines(string input)
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

                FAILangParser.CallsContext expressionContext = parser.calls();
                FAILangVisitor visitor = new FAILangVisitor();

                return visitor.VisitCalls(expressionContext).Select(x => Global.Instance.Evaluate(x)).ToArray();
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