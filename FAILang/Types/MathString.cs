using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    class MathString : IType
    {
        public string TypeName => "String";
        public string value;

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
    }
}
