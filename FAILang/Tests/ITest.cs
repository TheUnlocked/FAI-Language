using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Tests
{
    interface ITest
    {
        (string, object)[] Assertions { get; }
    }
}
