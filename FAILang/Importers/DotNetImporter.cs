using FAILang.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FAILang.Importers
{
    public class DotNetImporter : IImporter
    {
        public string[] FileExtensions => new string[] { ".dll" };

        public bool TryImport(string path, FAI fai, Global globals)
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
                        ExternalFunction func = (ExternalFunction)method.CreateDelegate(typeof(ExternalFunction));
                        globals.globalVariables[attr.FunctionName] = new ExternFunction(func, attr.ArgList);
                    }
                }
            }
            
            return true;
        }
    }
}
