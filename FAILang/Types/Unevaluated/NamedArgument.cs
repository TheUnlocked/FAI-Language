using FAILang.Types.Unevaluated.Passthrough;
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
        public Global globalEnvironment;

        public NamedArgument(string name, Global globalEnvironment, Namespace targetNamespace = null)
        {
            this.name = name;
            this.targetNamespace = targetNamespace;
            this.globalEnvironment = globalEnvironment;
        }

        public IType Evaluate(Scope scope)
        {
            IType result;
            if (targetNamespace != null)
            {
                result = targetNamespace?[name];
                if (result is Error)
                {
                    return new Error("InvalidName", $"The name {name} does not exist in namespace {targetNamespace.Address}.");
                }
                return result;
            }
            result = scope[name];
            if (result is Error)
            {
                return globalEnvironment.GlobalScope?[name];
            }
            if (result is IUnevaluated)
            {
                return new BakedExpression(result, scope);
            }
            return result;
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + name.GetHashCode();
            return hash;
        }
    }
}
