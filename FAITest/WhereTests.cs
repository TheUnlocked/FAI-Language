using FAILang.Types;
using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FAILang.Tests
{
    public class WhereTests : IDisposable
    {
        Global env;
        public WhereTests()
        {
            FAI.ClearInstance();
            env = new Global();
        }

        public void Dispose()
        {
            FAI.ClearInstance();
        }

        public static IEnumerable<object[]> OrderOfOperationsData()
        {
            yield return new object[] { "let x = 5 in y where y = x", new Number(5) };
            yield return new object[] { "(let x = 5 in y) where y = x", new Error() };
            yield return new object[] { "let x = 5 in (y where y = x)", new Number(5) };
            yield return new object[] { "let x = 5 in x + y where y = x + 2", new Number(12) };
            yield return new object[] { "x where x = y where y = z where z = 3", new Number(3) };
        }

        public static IEnumerable<object[]> LazyVariablesData()
        {
            yield return new object[] { "x where y = 2, x = y", new Number(2) };
            yield return new object[] { "x where x = y, y = 2", new Number(2) };
            yield return new object[] { "x where x = y, y = 2", new Number(2) };
        }

        [Theory]
        [MemberData(nameof(OrderOfOperationsData))]
        public void OrderOfOperations(string input, IType expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            if (expected is Error)
            {
                Assert.IsType<Error>(result);
            }
            else
            {
                Assert.Equal(expected, result);
            }
        }

        [Theory]
        [MemberData(nameof(LazyVariablesData))]
        public void LazyVariables(string input, IType expected)
        {
            var result = FAI.Instance.InterpretLines(env, input)[0];
            if (expected is Error)
            {
                Assert.IsType<Error>(result);
            }
            else
            {
                Assert.Equal(expected, result);
            }
        }
    }
}
