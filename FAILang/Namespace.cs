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
        public static Namespace Root { get; } = new Namespace("");

        static Namespace() {
            Root.Variables.Add("i", new Number(System.Numerics.Complex.ImaginaryOne));
        }

        public Namespace(string address) : base(null, new Dictionary<string, IType>())
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
                    var tempNS = new Namespace(current.Address == "" ? name : $"{current.Address}::{name}");
                    current.children.Add(name, tempNS);
                    current = tempNS;
                }
            }
            return current;
        }
    }
}
