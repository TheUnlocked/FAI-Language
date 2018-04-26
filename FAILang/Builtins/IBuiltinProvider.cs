using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Builtins
{
    public interface IBuiltinProvider
    {
        (string, IType)[] GetBuiltins();
        string[] GetReservedNames();
    }
}
