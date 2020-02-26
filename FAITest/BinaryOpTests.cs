using FAILang.Types;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace FAILang.Tests
{
    public class BinaryOpTests : IDisposable
    {
        Global env;
        public BinaryOpTests()
        {
            FAI.ClearInstance();
            env = new Global();
            FAI.Instance.InterpretLines(env, "x = 10");
        }

        public void Dispose()
        {
            FAI.ClearInstance();
        }

        public static IEnumerable<object[]> ExponentData()
        {
            yield return new object[] { "2^3^3", (Complex)134217728 };
            yield return new object[] { "-2^3", (Complex)(-8) };
            yield return new object[] { "2^-2", (Complex)0.25 };
            yield return new object[] { "2^-2^2", (Complex)0.0625 };
            yield return new object[] { "2x^2", (Complex)200 };
            yield return new object[] { "-2x^2", (Complex)(-200) };
            yield return new object[] { "-2x^-2", (Complex)(-0.02) };
            yield return new object[] { "2x^2x", (Complex)2e20 };
        }

        [Theory]
        [MemberData(nameof(ExponentData))]
        public void Exponent(string input, Complex expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            Assert.IsType<Number>(result);
            Assert.Equal(expected, ((Number)result).value);
        }

        public static IEnumerable<object[]> AdjacentMultiplyData()
        {
            yield return new object[] { "2 2", (Complex)2 };
            yield return new object[] { "2x", (Complex)20 };
            yield return new object[] { "x x", (Complex)10 };
        }

        [Theory]
        [MemberData(nameof(AdjacentMultiplyData))]
        public void AdjacentMultiply(string input, Complex expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[^1];
            Assert.IsType<Number>(result);
            Assert.Equal(expected, ((Number)result).value);
        }
    }
}
