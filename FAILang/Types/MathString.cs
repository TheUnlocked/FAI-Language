using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    class MathString : IType, IIndexable
    {
        public string TypeName => "String";
        public int Length => value.Length;

        public readonly string value;

        public MathString(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"\"{value.ToString()}\"";
        }

        public override bool Equals(object obj)
        {
            MathString str = obj as MathString;
            if (str == null)
                return false;
            return value.Equals(str.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<string>.Default.GetHashCode(value);
        }

        public IType IndexRange(int left_b, int right_b) => new MathString(new String(value.Skip(left_b).SkipLast(Length-right_b-1).ToArray()));

        public IType Index(int index) => new MathString(value[index].ToString());
    }
}
