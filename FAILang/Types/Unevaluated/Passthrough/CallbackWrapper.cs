using System;
using System.Collections.Generic;
using System.Linq;

namespace FAILang.Types.Unevaluated.Passthrough
{
    public class CallbackWrapper : IUnevaluated, IPassthrough
    {
        public string TypeName => "CallbackWrapper";
        public IType PassthroughExpression { get; private set; }

        public List<Action<IType>> callbacks = new List<Action<IType>>();

        public CallbackWrapper(IType expr, params Action<IType>[] cbs)
        {
            callbacks = cbs.ToList();
            PassthroughExpression = expr;
        }

        public IType Evaluate(Scope scope)
        {
            while (PassthroughExpression is CallbackWrapper cw)
            {
                PassthroughExpression = cw.PassthroughExpression;
                callbacks.AddRange(cw.callbacks);
            }
            if (PassthroughExpression is Union un)
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
            else if (PassthroughExpression is IUnevaluated uneval)
            {
                return new CallbackWrapper(uneval.Evaluate(scope), callbacks.ToArray()).Evaluate(scope);
            }
            else
            {
                foreach (Action<IType> cb in callbacks)
                {
                    cb.Invoke(PassthroughExpression);
                }
                return PassthroughExpression;
            }
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + EqualityComparer<List<Action<IType>>>.Default.GetHashCode(callbacks);
            hash = hash * 1532528149 + PassthroughExpression.GetHashCode();
            return hash;
        }
    }
}
