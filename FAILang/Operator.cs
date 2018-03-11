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
        [Description("+")]
        ADD,
        [Description("-")]
        SUBTRACT,
        [Description("*")]
        MULTIPLY,
        [Description("/")]
        DIVIDE,
        [Description("%")]
        MODULO,
        [Description("^")]
        EXPONENT,
        [Description("=")]
        EQUALS,
        [Description("~=")]
        NOT_EQUALS,
        [Description(">")]
        GREATER,
        [Description("<")]
        LESS,
        [Description(">=")]
        GR_EQUAL,
        [Description("<=")]
        LE_EQUAL
    }
}
