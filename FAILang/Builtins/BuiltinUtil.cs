using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Builtins
{
    public static class BuiltinUtil
    {
        public static IType FAIValue(string expression)
        {
            Global newEnv = new Global();
            var output = FAI.Instance.InterpretLines(newEnv, expression);
            return output[output.Length-1];
        }
    }
}
