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
    public enum Operator
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
    public static class OperatorExtension { public static string ToDisplayString(this Operator op) { switch(op) {
                case Operator.ADD:
                    return "+";
                case Operator.SUBTRACT:
                    return "-";
                case Operator.MULTIPLY:
                    return "*";
                case Operator.DIVIDE:
                    return "/";
                case Operator.MODULO:
                    return "%";
                case Operator.EXPONENT:
                    return "^";
                case Operator.EQUALS:
                    return "=";
                case Operator.NOT_EQUALS:
                    return "~=";
                case Operator.GREATER:
                    return ">";
                case Operator.LESS:
                    return "<";
                case Operator.GR_EQUAL:
                    return ">=";
                case Operator.LE_EQUAL:
                    return "<=";
                default:
                    return "";
            } } }
}
