using System;
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
        public Namespace targetNamespace;

        public NamedArgument(string name, Namespace targetNamespace = null)
        {
            this.name = name;
            this.targetNamespace = targetNamespace;
        }

        public IType Evaluate(Scope scope)
        {
            if (targetNamespace != null)
            {
                var result = targetNamespace?[name];
                if (result is Error)
                {
                    return new Error("InvalidName", $"The name {name} does not exist in namespace {targetNamespace.Address}.");
                }
                return result;
            }
            return scope[name];
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + name.GetHashCode();
            return hash;
        }
    }
}
