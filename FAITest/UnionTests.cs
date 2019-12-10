using FAILang.Types;
using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FAILang.Tests
{
    public class UnionTests : IDisposable
    {
        Global env;
        public UnionTests()
        {
            FAI.ClearInstance();
            env = new Global();
        }

        public void Dispose() {
            FAI.ClearInstance();
        }

        public static IEnumerable<object[]> DevolveToValueData()
        {
            yield return new object[] { "(1 | undefined)", new Number(1) };
            yield return new object[] { "(undefined | undefined)", Undefined.Instance };
            yield return new object[] { "(x -> { undefined if ~(x is undefined); x otherwise)((1 | 2 | 3))", Undefined.Instance };
            yield return new object[] { "(1 | 1.0 | 1+0i)", new Number(1) };
            yield return new object[] { "(x -> {x[0] if x[1..] is (,); (x[0] | self(x[1..])) otherwise)((1, 1, undefined, 1))", new Number(1) };
        }

        [Theory]
        [MemberData(nameof(DevolveToValueData))]
        public void DevolveToValue(string input, IType expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> MultiplicativeCardinalityData()
        {
            yield return new object[] { "(1 | 2) +- (3 | 4) +- (5 | 6)", 16 };
            yield return new object[] { "f(f(f(1, 2), 3), 4) where f = (x, y) -> (x | x-y)", 8 };
        }

        [Theory]
        [MemberData(nameof(MultiplicativeCardinalityData))]
        public void MultiplicativeCardinality(string input, int cardinality)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            Assert.IsType<Union>(result);
            Assert.Equal(cardinality, (result as Union).values.Length);
        }
    }
}
