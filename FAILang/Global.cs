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
        internal Dictionary<string, IType> _globalVariables = new Dictionary<string, IType>();
        public Namespace Namespace { get; set; } = Namespace.GlobalNamespace.Instance;
        private Scope _globalScope;
        public Scope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    _globalScope = new Scope(Namespace, _globalVariables);
                }
                return _globalScope;
            }
            set
            {
                _globalScope = value;
            }
        }

        public readonly List<string> reservedNames = new List<string>
        {
            "i",
            "true",
            "false",
            "undefined",
            "update",
            "memo",
            "self",
            "if",
            "otherwise",
            "is"
        };

        public Global()
        {
            LoadBuiltins(FAI.Instance.builtinProviders.ToArray());
        }

        private void LoadBuiltins(params IBuiltinProvider[] builtinProviders)
        {
            foreach (var builtinProvider in builtinProviders)
            {
                var ns = Namespace.GlobalNamespace.Instance.GetSubNamespace(builtinProvider.NamespacePath);
                foreach (var pair in builtinProvider.GetBuiltins())
                {
                    ns.Variables[pair.Item1] = pair.Item2;
                }
                foreach (string name in builtinProvider.GetReservedNames())
                {
                    reservedNames.Add(name);
                }
            }
        }

        private static readonly Scope emptyScope = new Scope(null, new Dictionary<string, IType>());

        public IType Evaluate(IType expr)
        {
            var exprMem = expr;
            while (expr is IUnevaluated u)
            {
                if (u is Union un)
                {
                    if (!un.values.Any(x => x is IUnevaluated))
                    {
                        expr = un.Evaluate(emptyScope);
                        break;
                    }
                }
                expr = u.Evaluate(GlobalScope);
                if (expr.GetType() == exprMem.GetType() && expr.GetHashCode() == exprMem.GetHashCode())
                {
                    return new Error("InfiniteRecursion", "The entered expression will recurse infinitely, and has been terminated");
                }
            }
            return expr;
        }
    }
}
