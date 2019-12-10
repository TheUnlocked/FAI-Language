using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FAILang.Builtins;
using FAILang.Types;

namespace FAILang.Tests
{
    public class FunctionTests
    {
        Global env;
        public FunctionTests()
        {
            FAI.ClearInstance();
            FAI.Instance.ProvideBuiltins(new NumberBuiltinProvider());
            env = new Global();
            FAI.Instance.InterpretLines(env, @"
            using std/math
            ");
        }

        public void Dispose()
        {
            FAI.ClearInstance();
        }

        public static IEnumerable<object[]> TupleExpansionData()
        {
            yield return new object[] { "sqrt(((1,) | (4,))...)", new Union(new IType[] { new Number(1), new Number(2) }) };
        }

        [Theory]
        [MemberData(nameof(TupleExpansionData))]
        public void TupleExpansion(string input, IType expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            Assert.Equal(expected.ToString(), result.ToString());
        }
    }
}
