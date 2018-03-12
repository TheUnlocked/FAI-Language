using FAILang.Types;
using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FAILang
{
    public enum BinaryOperator
    {
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MODULO,
        EXPONENT,
        EQUALS,
        NOT_EQUALS,
        GREATER,
        LESS,
        GR_EQUAL,
        LE_EQUAL
    }
    public enum UnaryOperator
    {
        NOT,
        NEGATIVE
    }

    public static class OperatorExtension {
        public static string ToDisplayString(this BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.ADD:
                    return "+";
                case BinaryOperator.SUBTRACT:
                    return "-";
                case BinaryOperator.MULTIPLY:
                    return "*";
                case BinaryOperator.DIVIDE:
                    return "/";
                case BinaryOperator.MODULO:
                    return "%";
                case BinaryOperator.EXPONENT:
                    return "^";
                case BinaryOperator.EQUALS:
                    return "=";
                case BinaryOperator.NOT_EQUALS:
                    return "~=";
                case BinaryOperator.GREATER:
                    return ">";
                case BinaryOperator.LESS:
                    return "<";
                case BinaryOperator.GR_EQUAL:
                    return ">=";
                case BinaryOperator.LE_EQUAL:
                    return "<=";
                default:
                    return "";
            }
        }
        public static string ToDisplayString(this UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.NEGATIVE:
                    return "-";
                case UnaryOperator.NOT:
                    return "~";
                default:
                    return "";
            }
        }
    }
}
