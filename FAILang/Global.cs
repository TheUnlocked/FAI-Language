using FAILang.Builtins;
using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FAILang
{
    public static class Global
    {
        public static Dictionary<string, Function> functions = new Dictionary<string, Function>();
        public static Dictionary<string, IType> variables = new Dictionary<string, IType>();

        public static readonly List<string> reservedNames = new List<string>
        {
            "i",
            "true",
            "false",
            "void",
            "lambda",
            "update",
            "memo",
            "self"
        };

        public static void LoadBuiltins(params IBuiltinProvider[] builtinProviders)
        {
            foreach (var builtinProvider in builtinProviders)
            {
                foreach (var pair in builtinProvider.GetBuiltins())
                {
                    functions[pair.Item1] = pair.Item2;
                }
                foreach (string name in builtinProvider.GetReservedNames())
                {
                    reservedNames.Add(name);
                }
            }
        }
    }
}
