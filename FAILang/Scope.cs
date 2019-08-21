using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang
{
    public class Scope
    {
        protected Scope parent;
        protected IReadOnlyDictionary<string, IType> variables;

        public Scope(Scope parent, IReadOnlyDictionary<string, IType> variables)
        {
            this.parent = parent;
            this.variables = variables;
        }

        public virtual IType this[string varName]
        {
            get
            {
                if (variables.TryGetValue(varName, out var val))
                {
                    return val;
                }
                else if (parent != null)
                {
                    return parent[varName];
                }
                else
                {
                    return new Error("InvalidName", $"The name {varName} does not exist in scope.");
                }
            }
        }

        public bool TryGetVar(string varName, out IType value)
        {
            if (variables.TryGetValue(varName, out value))
            {
                return true;
            }
            else
            {
                if (parent == null)
                {
                    return false;
                }
                else
                {
                    return parent.TryGetVar(varName, out value);
                }
            }
        }

        public bool HasVar(string varName)
        {
            return variables.ContainsKey(varName) || (parent != null && parent.HasVar(varName));
        }
    }
}
