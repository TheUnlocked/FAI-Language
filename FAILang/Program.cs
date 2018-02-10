using System;
using Antlr4.Runtime;
using FAILang.Types;
using FAILang.Types.Unevaluated;

namespace FAILang
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.InitalizeGlobals();

            // Read-eval-print loop
            while (true) {
                try
                {
                    string input = Console.ReadLine();
                    AntlrInputStream inputStream = new AntlrInputStream(input);

                    FAILangLexer lexer = new FAILangLexer(inputStream);
                    CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
                    FAILangParser parser = new FAILangParser(commonTokenStream);

                    FAILangParser.CallContext expressionContext = parser.call();
                    FAILangVisitor visitor = new FAILangVisitor();

                    IType val = visitor.VisitCall(expressionContext);
                    if (val is IUnevaluated e)
                        Console.WriteLine(e.Evaluate(Global.variables));
                    else if (val != null)
                        Console.WriteLine(val);
                }
                catch (StackOverflowException)
                {
                    Console.WriteLine(new Error("StackOverflow", "The call stack has overflowed"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }

}