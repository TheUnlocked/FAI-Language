using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang
{
    public class Namespace : Scope
    {
        private readonly Dictionary<string, Namespace> children = new Dictionary<string, Namespace>();
        public Dictionary<string, IType> Variables => (Dictionary<string, IType>)variables;
        public string Address { get; }

        public Namespace(string address) : base(GlobalNamespace.Instance, new Dictionary<string, IType>())
        {
            Address = address;
        }

        public Namespace GetSubNamespace(string name)
        {
            if (children.TryGetValue(name, out var ns))
            {
                return ns;
            }
            else
            {
                var tempNS = new Namespace(Address == "" ? name : $"{Address}::{name}");
                children.Add(name, tempNS);
                return tempNS;
            }
        }
        public Namespace GetSubNamespace(IEnumerable<string> namespacePath)
        {
            var current = this;
            foreach (var name in namespacePath)
            {
                if (current.children.TryGetValue(name, out var ns))
                {
                    current = ns;
                }
                else
                {
                    var tempNS = new Namespace(Address == "" ? name : $"{Address}::{name}");
                    current.children.Add(name, tempNS);
                    current = tempNS;
                }
            }
            return current;
        }

        public class GlobalNamespace : Namespace {
            public static GlobalNamespace Instance { get; } = new GlobalNamespace();

            private readonly List<Namespace> includedNamespaces = new List<Namespace>();

            private GlobalNamespace() : base("") { }

            public IType IncludeNamespace(Namespace ns)
            {
                if (includedNamespaces.Contains(ns))
                {
                    return new Error("IncludeError", "A namespace cannot be included more than once.");
                }
                includedNamespaces.Add(ns);
                return null;
            }

            public override IType this[string varName]
            {
                get
                {
                    if (TryGetVar(varName, out var val))
                    {
                        return val;
                    }
                    else
                    {
                        foreach (var ns in includedNamespaces)
                        {
                            if (ns.TryGetVar(varName, out var val2))
                            {
                                return val2;
                            }
                        }
                        return new Error("InvalidName", $"The name {varName} does not exist in scope.");
                    }
                }
            }
        }
    }
}
