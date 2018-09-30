using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using FAILang.Types;
using FAILang.Types.Unevaluated;

namespace FAILang.Builtins
{
    class NumberBuiltinProvider : IBuiltinProvider
    {
        private static ExternalFunction ValidateType<T>(Func<T, IType> f, Func<IType, IType> fail) where T : IType
        {
            return x =>
            {
                if (x[0] is T t)
                    return f.Invoke(t);
                return fail.Invoke(x[0]);
            };
        }

        private static ExternFunction REAL = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value.Real),
                x => new Error("WrongType", $"{x} has no real component")),
                "c");
        private static ExternFunction IMAGINARY = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value.Imaginary * Complex.ImaginaryOne),
                x => new Error("WrongType", $"{x} has no imaginary component")),
                "c");
        private static ExternFunction FLOOR = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Floor(x.value.Real), Math.Floor(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to floor")),
                "c");
        private static ExternFunction CEILING = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Ceiling(x.value.Real), Math.Ceiling(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to ceiling")),
                "c");
        private static ExternFunction ROUND = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Round(x.value.Real), Math.Round(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to round")),
                "c");
        private static ExternFunction ABS = new ExternFunction(x => new UnaryOperatorExpression(UnaryOperator.ABS, x[0]), "x");
        private static ExternFunction SQRT = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Sqrt(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sqrt")),
                "a");
        private static ExternFunction CONJUGATE = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Conjugate(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to conjugate")),
                "c");
        private static ExternFunction SIN = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Sin(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sin")),
                "theta");
        private static ExternFunction COS = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Cos(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to cos")),
                "theta");
        private static ExternFunction TAN = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Tan(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to tan")),
                "theta");
        private static ExternFunction ACOS = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Acos(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to cos^-1 (acos)")),
                "s");
        private static ExternFunction ASIN = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Asin(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sin^-1 (asin)")),
                "s");
        private static ExternFunction ATAN = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Atan(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to tan^-1 (atan)")),
                "s");
        private static ExternFunction LOG10 = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Log10(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to log10")),
                "a");
        private static ExternFunction LN = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Log(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to ln")),
                "a");
        private static ExternFunction LOG = new ExternFunction(xs => {
                    if (xs[0] is Number a && xs[1] is Number b)
                        return new Number(Complex.Log(a.value, b.value.Magnitude));
                    return new Error("WrongType", $"{xs[0]} and {xs[1]} are not valid inputs to log");
                }, "a", "b");
        
        private static Number PI = new Number(Math.PI);
        private static Number E = new Number(Math.E);
        private static Number DEGREES_TO_RADIANS = new Number(2 * Math.PI / 360);
        private static Number RADIANS_TO_DEGREES = new Number(360 / 2 * Math.PI);

        public (string, IType)[] GetBuiltins() => new(string, IType)[] {
                ("real", REAL),
                ("imaginary", IMAGINARY),
                ("floor", FLOOR),
                ("ceiling", CEILING),
                ("round", ROUND),
                ("abs", ABS),
                ("sqrt", SQRT),
                ("conjugate", CONJUGATE),
                ("sin", SIN),
                ("cos", COS),
                ("tan", TAN),
                ("acos", ACOS),
                ("asin", ASIN),
                ("atan", ATAN),
                ("log10", LOG10),
                ("ln", LN),
                ("log", LOG),
                
                ("pi", PI),
                ("e", E),
                ("degrees_to_radians", DEGREES_TO_RADIANS),
                ("radians_to_degrees", RADIANS_TO_DEGREES)
            };

        public string[] GetReservedNames() => new string[] {
            "real",
            "imaginary",
            "floor",
            "ceiling",
            "round",
            "abs",
            "sqrt",
            "conjugate",
            "sin",
            "cos",
            "tan",
            "acos",
            "asin",
            "atan",
            "log10",
            "ln",
            "log",

            "pi",
            "e",
            "degrees_to_radians",
            "radians_to_degrees"
        };
    }
}
