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
        PLUS_MINUS,
        MULTIPLY,
        DIVIDE,
        EXPONENT,
        CONCAT,
        IS
    }
    public enum RelationalOperator
    {
        EQUALS,
        NOT_EQUALS,
        GREATER,
        LESS,
        GREATER_EQUAL,
        LESS_EQUAL
    }
    public enum UnaryOperator
    {
        NOT,
        NEGATIVE,
        PLUS_MINUS,
        ABS
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
                case BinaryOperator.PLUS_MINUS:
                    return "+-";
                case BinaryOperator.MULTIPLY:
                    return "*";
                case BinaryOperator.DIVIDE:
                    return "/";
                case BinaryOperator.EXPONENT:
                    return "^";
                case BinaryOperator.CONCAT:
                    return "||";
                case BinaryOperator.IS:
                    return "is";
                default:
                    return "";
            }
        }
        public static string ToDisplayString(this RelationalOperator op)
        {
            switch (op)
            {
                case RelationalOperator.EQUALS:
                    return "=";
                case RelationalOperator.NOT_EQUALS:
                    return "~=";
                case RelationalOperator.GREATER:
                    return ">";
                case RelationalOperator.LESS:
                    return "<";
                case RelationalOperator.GREATER_EQUAL:
                    return ">=";
                case RelationalOperator.LESS_EQUAL:
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
                case UnaryOperator.PLUS_MINUS:
                    return "+-";
                case UnaryOperator.NOT:
                    return "~";
                case UnaryOperator.ABS:
                    return "|x|";
                default:
                    return "";
            }
        }
    }
}
