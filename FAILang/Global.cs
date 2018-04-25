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
    public class Global
    {
        private static Global _instance;
        public static Global Instance {
            get {
                if (_instance == null)
                    _instance = new Global();
                return _instance;
            }
        }

        public static void ResetGlobals()
        {
            _instance = new Global();
        }

        public Dictionary<string, IType> globalVariables = new Dictionary<string, IType>();

        public readonly List<string> reservedNames = new List<string>
        {
            "i",
            "true",
            "false",
            "undefined",
            "lambda",
            "update",
            "memo",
            "self",
            "if",
            "otherwise",
            "is"
        };

        public void LoadBuiltins(params IBuiltinProvider[] builtinProviders)
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

        static readonly Dictionary<string, IType> noVariables = new Dictionary<string, IType>();

        public IType Evaluate(IType expr)
        {
            var exprMem = expr;
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
                if (expr.GetType() == exprMem.GetType() && expr.GetHashCode() == exprMem.GetHashCode())
                {
                    return new Error("InfiniteRecursion", "The entered expression will recurse infinitely, and has been terminated");
                }
            }
            return expr;
        }
    }
}
