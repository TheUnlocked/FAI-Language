using FAILang.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace FAILang.Importers
{
    public class DotNetImporter : IImporter
    {
        public string[] FileExtensions => new string[] { ".dll" };

        public bool TryImport(string path, FAI fai)
        {
            Assembly dll;

            try
            {
                dll = Assembly.LoadFile(Path.GetFullPath(path));
            }
            catch (Exception)
            {
                return false;
            }

            foreach (Type type in dll.GetExportedTypes())
            {

                foreach (var method in type.GetMethods()) {
                    var attr = (FAIMethodAttribute)method.GetCustomAttribute(typeof(FAIMethodAttribute));
                    if (attr != null)
                    {
                        string[] namePath = attr.FunctionName.Split(":");
                        ExternalFunction func = (ExternalFunction)method.CreateDelegate(typeof(ExternalFunction));
                        Namespace.GlobalNamespace.Instance.GetSubNamespace(namePath.SkipLast(1)).Variables[attr.FunctionName] = new ExternFunction(func, attr.ArgList);
                    }
                }
            }
            
            return true;
        }
    }
}
