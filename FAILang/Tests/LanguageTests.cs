using System;
using System.Collections.Generic;
using System.Text;
using FAILang.Types;
using FAILang.Types.Unevaluated;

namespace FAILang.Tests
{
    public class LanguageTests : ITest
    {
        public (string, object)[] Assertions => new(string, object)[]{
            ("1", new Number(1)),
            ("-i", new Number(new System.Numerics.Complex(0, -1)))
        };
    }
}
