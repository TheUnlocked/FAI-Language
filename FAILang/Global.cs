using FAILang.Builtins;
using FAILang.Types;
using FAILang.Types.Unevaluated;
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
        public static Dictionary<string, IType> globalVariables = new Dictionary<string, IType>();

        public static readonly Dictionary<string, IType> noVariables = new Dictionary<string, IType>();

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
                    globalVariables[pair.Item1] = pair.Item2;
                }
                foreach (string name in builtinProvider.GetReservedNames())
                {
                    reservedNames.Add(name);
                }
            }
        }

        public static IType Evaluate(IType expr)
        {
            while (expr is IUnevaluated u)
            {
                if (u is Union un)
                {
                    if (!un.values.Any(x => x is IUnevaluated))
                    {
                        expr = un.Evaluate(noVariables);
                        break;
                    }
                }
                expr = u.Evaluate(noVariables);
            }
            return expr;
        }
    }
}
