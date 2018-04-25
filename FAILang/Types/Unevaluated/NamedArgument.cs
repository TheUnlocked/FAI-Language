﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types.Unevaluated
{
    public class NamedArgument : IUnevaluated
    {
        public string TypeName => "UnevaluatedName";

        public string name;
        public NamedArgument(string name)
        {
            this.name = name;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            if (lookups.ContainsKey(name))
            {
                IType val = lookups[name];
                if (val is IUnevaluated)
                    return (val as IUnevaluated).Evaluate(lookups);
                return val;
            }
            else if (Global.Instance.globalVariables.ContainsKey(name))
                return Global.Instance.globalVariables[name];
            return new Error("InvalidName", $"The name {name} is neither globally defined nor an argument.");
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + name.GetHashCode();
            return hash;
        }
    }
}
