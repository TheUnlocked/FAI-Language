using FAILang.Builtins;
using FAILang.Importers;
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
                new TypesBuiltinProvider(),
                new FunctionBuiltinProvider()
            );
            FAI.Instance.LoadImporters(
                new DotNetImporter(),
                new FAIImporter()
            );
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

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
                    input += '\n' + Console.ReadLine();

                foreach (var val in fai.InterpretLines(env, input))
                    if (val != null)
                        Console.WriteLine(val);
            }
        }
    }
}
