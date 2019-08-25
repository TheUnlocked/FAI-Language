using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types.Unevaluated.Passthrough
{
    interface IPassthrough : IType
    {
        IType PassthroughExpression { get; }
        IType ReplacePassthroughExpression(IType replacement);
    }

    static class PassthroughExtension
    {
        public static IType DeepGetPassthroughObject(this IPassthrough passthrough)
        {
            IType result = passthrough.PassthroughExpression;
            while (result is IPassthrough pt)
            {
                result = pt.PassthroughExpression;
            }
            return result;
        }

        public static IType DeepReplacePassthroughObject(this IPassthrough passthrough, IType replacement)
        {
            Stack<IPassthrough> passthroughStack = new Stack<IPassthrough>();
            passthroughStack.Push(passthrough);
            IType current = passthrough.PassthroughExpression;
            while (current is IPassthrough pt)
            {
                passthroughStack.Push(pt);
                current = pt.PassthroughExpression;
            }
            current = passthroughStack.Pop().ReplacePassthroughExpression(replacement);
            while (passthroughStack.Count > 0)
            {
                current = passthroughStack.Pop().ReplacePassthroughExpression(current);
            }
            return current;
        }
    }
}
