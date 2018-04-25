using System;
using System.Collections.Generic;
using System.Linq;

namespace FAILang.Types.Unevaluated
{
    public class CallbackWrapper : IUnevaluated
    {
        public string TypeName => "CallbackWrapper";
        public List<Action<IType>> callbacks = new List<Action<IType>>();
        IType expression;

        public CallbackWrapper(IType expr, params Action<IType>[] cbs)
        {
            callbacks = cbs.ToList();
            expression = expr;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            while (expression is CallbackWrapper cw)
            {
                expression = cw.expression;
                callbacks.AddRange(cw.callbacks);
            }
            if (expression is Union un)
            {
                List<IType> items = new List<IType>();
                return new Union(un.values.Select(v =>
                    new CallbackWrapper(v, x =>
                        {
                            items.Add(x);
                            if (items.Count == un.values.Length)
                            {
                                var u = new Union(items.ToArray());
                                foreach (Action<IType> cb in callbacks)
                                {
                                    cb.Invoke(u);
                                }
                            }
                        })).ToArray());
            }
            else if (expression is IUnevaluated uneval)
            {
                return new CallbackWrapper(uneval.Evaluate(lookups), callbacks.ToArray()).Evaluate(lookups);
            }
            else
            {
                foreach (Action<IType> cb in callbacks)
                {
                    cb.Invoke(expression);
                }
                return expression;
            }
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + EqualityComparer<List<Action<IType>>>.Default.GetHashCode(callbacks);
            hash = hash * 1532528149 + expression.GetHashCode();
            return hash;
        }
    }
}
