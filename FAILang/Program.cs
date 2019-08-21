//#define RUN_TESTS

using FAILang.Builtins;
using FAILang.Importers;
using FAILang.Tests;
using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FAILang
{
    class Program
    {
        static void Main(string[] args)
        {
            FAI fai = FAI.Instance;
            fai.ProvideBuiltins(
                new NumberBuiltinProvider(),
                new CollectionBuiltinProvider(),
                new TypesBuiltinProvider()
            );
            FAI.Instance.LoadImporters(new DotNetImporter());
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

#if RUN_TESTS
            {
                Global testEnv = new Global();

                foreach (ITest testPackage in new ITest[] { new LanguageTests() })
                {
                    object firstIfOnly(IType[] list) => list.Length == 0 ? null : (list.Length == 1 ? list[0] : (object)list);
                    foreach (var assertion in testPackage.Assertions)
                    {
                        try
                        {
                            Debug.Assert(firstIfOnly(fai.InterpretLines(testEnv, assertion.Item1)).Equals(assertion.Item2));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }
            }
#endif

            Global env = new Global();

            if (!File.Exists("input.fai"))
                File.Create("input.fai").Close();
            string fileInput = File.ReadAllText("input.fai");
            if (fileInput.Length > 0)
            {
                Console.Write(fileInput + "\n\n");
                foreach (var val in fai.InterpretLines(env, fileInput))
                    if (val is Error)
                        Console.WriteLine(val);
            }

            // Read-eval-print loop
            while (true)
            {
                string input = Console.ReadLine();
                while (input.EndsWith("  "))
                    input += Console.ReadLine();

                foreach (var val in fai.InterpretLines(env, input))
                    if (val != null)
                        Console.WriteLine(val);
            }
        }
    }
}
