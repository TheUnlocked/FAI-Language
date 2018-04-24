using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    struct MathString : IOperable, IIndexable
    {
        public string TypeName => "String";
        public int Length => value.Length;

        public readonly string value;

        public MathString(string value)
        {
            this.value = value;
        }

        public Dictionary<BinaryOperator, Func<IOperable, IType>> BinaryOperators => new Dictionary<BinaryOperator, Func<IOperable, IType>>()
        {
            {BinaryOperator.CONCAT, OpConcat}
        };

        public Dictionary<RelationalOperator, Func<IOperable, MathBool>> RelativeOperators => null;
        public Dictionary<UnaryOperator, Func<IType>> UnaryOperators => null;

        private IType OpConcat(IOperable other)
        {
            switch (other)
            {
                case MathString str:
                    return new MathString(value + str.value);
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return $"\"{value.ToString()}\"";
        }

        public override bool Equals(object obj)
        {
            if (obj is MathString str)
                return value.Equals(str.value);
            return false;
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<string>.Default.GetHashCode(value);
        }

        public IType IndexRange(int left_b, int right_b) => new MathString(new String(value.Skip(left_b).SkipLast(Length-right_b-1).ToArray()));

        public IType Index(int index) => new MathString(value[index].ToString());
    }
}
