using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FAILang
{
    public static class Global
    {
        public static Dictionary<string, Function> functions = new Dictionary<string, Function>();
        public static Dictionary<string, IType> variables = new Dictionary<string, IType>();

        public static readonly string[] reservedNames = new string[]
        {
            "i",
            "true",
            "false",
            "void",
            "lambda",
            "update",
            "memo",
            "self",

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
            "log"
        };

        public static void InitalizeGlobals()
        {
            ExternalFunction ValidateType<T>(Func<T, IType> f, Func<IType, IType> fail) where T : IType
            {
                return x =>
                {
                    if (x[0] is T t)
                    {
                        return f.Invoke(t);
                    }
                    return fail.Invoke(x[0]);
                };
            }

            // Number functions
            functions["real"] = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value.Real),
                x => new Error("WrongType", $"{x} has no real component")),
                "c");
            functions["imaginary"] = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value.Imaginary * Complex.ImaginaryOne),
                x => new Error("WrongType", $"{x} has no imaginary component")),
                "c");

            functions["floor"] = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Floor(x.value.Real), Math.Floor(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to floor")),
                "c");
            functions["ceiling"] = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Ceiling(x.value.Real), Math.Ceiling(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to ceiling")),
                "c");
            functions["round"] = new ExternFunction(ValidateType<Number>(
                x => new Number(new Complex(Math.Round(x.value.Real), Math.Round(x.value.Imaginary))),
                x => new Error("WrongType", $"{x} is not a valid input to round")),
                "c");

            functions["abs"] = new ExternFunction(ValidateType<Number>(
                x => new Number(x.value.Magnitude),
                x => new Error("WrongType", $"{x} has no absolute value")),
                "n");

            functions["sqrt"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Sqrt(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sqrt")),
                "a");
            functions["conjugate"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Conjugate(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to conjugate")),
                "c");

            functions["sin"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Sin(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sin")),
                "theta");
            functions["cos"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Cos(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to cos")),
                "theta");
            functions["tan"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Tan(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to tan")),
                "theta");

            functions["acos"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Acos(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to cos^-1 (acos)")),
                "s");
            functions["asin"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Asin(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to sin^-1 (asin)")),
                "s");
            functions["atan"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Atan(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to tan^-1 (atan)")),
                "s");

            functions["log10"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Log10(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to log10")),
                "a");
            functions["ln"] = new ExternFunction(ValidateType<Number>(
                x => new Number(Complex.Log(x.value)),
                x => new Error("WrongType", $"{x} is not a valid input to ln")),
                "a");
            functions["log"] = new ExternFunction(xs =>
                {
                    if (xs[0] is Number a && xs[1] is Number b)
                    {
                        return new Number(Complex.Log(a.value, b.value.Magnitude));
                    }
                    return new Error("WrongType", $"{xs[0]} and {xs[1]} are not valid inputs to log");
                },
                "a", "b");
        }
    }
}
