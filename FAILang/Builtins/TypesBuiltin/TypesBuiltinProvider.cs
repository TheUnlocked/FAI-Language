using System;
using System.Collections.Generic;
using System.Text;
using FAILang.Types;
using FAILang.Types.Unevaluated;
using FAILang.Types.Unevaluated.Passthrough;
using Tuple = FAILang.Types.Tuple;

namespace FAILang.Builtins
{
    class TypesBuiltinProvider : IBuiltinProvider
    {
        public string[] NamespacePath { get; } = new string[] { "std", "type" };

        private static ExternalFunction ValidateType<T>(Func<T, IType> f, Func<IType, IType> fail) where T : IType
        {
            return x =>
            {
                if (x[0] is T t)
                    return f.Invoke(t);
                return fail.Invoke(x[0]);
            };
        }

        private static ExternFunction ESTABLISH_CONSTRUCTOR = new ExternFunction(args =>
        {
            var constructorFunction = args[0];
            if (constructorFunction is Function f)
            {
                IType innerObject = f.expression;

                if (innerObject is IPassthrough passthroughObject)
                {
                    innerObject = passthroughObject.DeepGetPassthroughObject();
                }

                ExternFunction constructor = null;
                constructor = new ExternFunction(fargs => {
                    var result = f.Evaluate(fargs);
                    while (result is IPassthrough pt)
                    {
                        IType inner = pt.DeepGetPassthroughObject();
                        if (inner is IUnevaluated)
                        {
                            result = (pt as BakedExpression).Evaluate(null);
                        }
                        if (inner is Function innerObj)
                            return pt.DeepReplacePassthroughObject(new UnevaluatedTypedFunction(constructor, innerObj));
                    }
                    if (result is Function obj)
                    {
                        return new UnevaluatedTypedFunction(f, obj);
                    }
                    return result;
                }, f.memoize, f.elipsis, f.fparams);

                return constructor;
            }
            return new Error("InvalidConstructor", "A constructor must be a Function");
        }, "constructor");

        private static ExternFunction GET_CONSTRUCTOR = new ExternFunction(args =>
        {
            var obj = args[0];
            switch (obj)
            {
                case TypedFunction tf: return tf.constructor;
                case Function x: return FUNCTION;
                case Number x: return NUMBER;
                case MathBool x: return BOOLEAN;
                case MathString x: return STRING;
                case Vector x: return VECTOR;
                case Tuple x: return TUPLE;
            }
            return new Error("MissingConstructor", $"No constructor exists for type {obj.TypeName}");
        }, "object");

        public (string, IType)[] GetBuiltins() => new(string, IType)[] {
                ("makeConstructor", ESTABLISH_CONSTRUCTOR),
                ("getConstructor", GET_CONSTRUCTOR),
                ("Function", FUNCTION),
                ("Number", NUMBER),
                ("Boolean", BOOLEAN),
                ("String", STRING),
                ("Vector", VECTOR),
                ("Tuple", TUPLE)
            };

        private static ExternFunction FUNCTION = new ExternFunction(ValidateType<Function>(
                x => x,
                x => new Error("WrongType", $"A Function object cannot be constructed from a {x.TypeName}")),
                "func");

        private static ExternFunction NUMBER = new ExternFunction(ValidateType<Number>(
                x => x,
                x => new Error("WrongType", $"A Number object cannot be constructed from a {x.TypeName}")),
                "num");

        private static ExternFunction BOOLEAN = new ExternFunction(ValidateType<MathBool>(
                x => x,
                x => new Error("WrongType", $"A Boolean object cannot be constructed from a {x.TypeName}")),
                "bool");

        private static ExternFunction STRING = new ExternFunction(ValidateType<MathString>(
                x => x,
                x => new MathString(x.ToString())),
                "string");

        private static ExternFunction VECTOR = new ExternFunction(ValidateType<Vector>(
                x => x,
                x => new Error("WrongType", $"A Vector object cannot be constructed from a {x.TypeName}")),
                "vector");

        private static ExternFunction TUPLE = new ExternFunction(ValidateType<Tuple>(
                x => x,
                x => new Error("WrongType", $"A Tuple object cannot be constructed from a {x.TypeName}")),
                "tuple");
    }
}
