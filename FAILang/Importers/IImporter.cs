using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Importers
{
    public interface IImporter
    {
        string[] FileExtensions { get; }
        bool TryImport(string path, FAI fai);
    }
}
