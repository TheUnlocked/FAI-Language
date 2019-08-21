using System;
using System.Collections.Generic;
using System.Text;
using FAILang.Types;
using FAILang.Types.Unevaluated.Passthrough;
using Tuple = FAILang.Types.Tuple;

namespace FAILang.Builtins
{
    class TypesBuiltinProvider : IBuiltinProvider
    {
        public string[] NamespacePath { get; } = new string[] { "std", "types" };

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
            var constructor = args[0];
            if (constructor is Function f)
            {
                IType innerObject = f.expression;

            CheckPassthrough:
                if (innerObject is IPassthrough passthroughObject)
                {
                    innerObject = passthroughObject.PassthroughExpression;
                    goto CheckPassthrough;
                }

                if (innerObject is UnevaluatedFunction innerFunction)
                {
                    UnevaluatedTypedFunction newConstruction = new UnevaluatedTypedFunction(innerFunction);
                    f = new Function(f.fparams, newConstruction, f.scope, f.memoize, f.elipsis);
                    newConstruction.constructor = f;
                    return f;
                }
            }
            return new Error("InvalidConstructor", "A constructor must be a Function in the format <args1> -> <args2> -> <expression>");
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
                ("establishConstructor", ESTABLISH_CONSTRUCTOR),
                ("getConstructor", GET_CONSTRUCTOR),
                ("Function", FUNCTION),
                ("Number", NUMBER),
                ("Boolean", BOOLEAN),
                ("String", STRING),
                ("Vector", VECTOR),
                ("Tuple", TUPLE)
            };

        public string[] GetReservedNames() => new string[] {
            "Function",
            "Number",
            "Boolean",
            "String",
            "Vector",
            "Tuple"
        };


        private static ExternFunction FUNCTION = new ExternFunction(ValidateType<Function>(
                x => new Function(x.fparams, x.expression, x.scope, x.memoize, x.elipsis),
                x => new Error("WrongType", $"A Function object cannot be constructed from a {x.TypeName}")),
                "func");

        private static ExternFunction NUMBER = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value),
                x => new Error("WrongType", $"A Number object cannot be constructed from a {x.TypeName}")),
                "num");

        private static ExternFunction BOOLEAN = new ExternFunction(ValidateType<MathBool>(
                x => x.value ? MathBool.TRUE : MathBool.FALSE,
                x => new Error("WrongType", $"A Boolean object cannot be constructed from a {x.TypeName}")),
                "bool");

        private static ExternFunction STRING = new ExternFunction(ValidateType<MathString>(
                x => new MathString(x.value),
                x => new MathString(x.ToString())),
                "string");

        private static ExternFunction VECTOR = new ExternFunction(ValidateType<Vector>(
                x => new Vector(x.items),
                x => new Error("WrongType", $"A Vector object cannot be constructed from a {x.TypeName}")),
                "vector");

        private static ExternFunction TUPLE = new ExternFunction(ValidateType<Tuple>(
                x => new Tuple(x.items),
                x => new Error("WrongType", $"A Tuple object cannot be constructed from a {x.TypeName}")),
                "tuple");
    }
}
