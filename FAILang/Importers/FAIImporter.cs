using FAILang.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FAILang.Importers
{
    class FAIImporter : IImporter
    {
        public string[] FileExtensions => new string[] { ".fai" };

        public bool TryImport(string path, Global globalEnvironment)
        {
            Global newEnvironment = new Global();

            if (!File.Exists(path))
                return false;
            string fileInput = File.ReadAllText(path);
            if (fileInput.Length > 0)
            {
                var errors = FAI.Instance.InterpretLines(newEnvironment, fileInput).Where(x => x is Error);
                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                        Console.WriteLine($"{path}: {error}");
                    return false;
                }
            }
            foreach(var pair in newEnvironment._globalVariables)
            {
                globalEnvironment.Namespace.Variables[pair.Key] = pair.Value;
            }
            return true;
        }
    }
}
