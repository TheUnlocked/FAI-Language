using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAILang
{
    public class MultiScope : Scope
    {
        internal readonly List<Scope> bonusParents = new List<Scope>();

        public MultiScope(Scope[] parents, IReadOnlyDictionary<string, IType> variables) : base(parents[0], variables)
        {
            bonusParents = parents.Skip(1).ToList();
        }

        public MultiScope(Scope parent, IReadOnlyDictionary<string, IType> variables) : base(parent, variables)
        {
        }

        public IType AddParent(Scope scope)
        {
            if (bonusParents.Contains(scope))
            {
                return new Error("IncludeError", "A namespace cannot be included more than once.");
            }
            bonusParents.Add(scope);
            return null;
        }

        public override IType this[string varName]
        {
            get
            {
                var result = base[varName];
                if (result is Error)
                {
                    foreach(var parent in bonusParents){
                        result = parent[varName];
                        if (!(result is Error))
                            return result;
                    }
                }
                return result;
            }
        }
    }
}
