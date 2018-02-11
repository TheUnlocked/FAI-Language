using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace FAILang.Types
{
    public class Number : IType
    {
        public string TypeName => "Number";

        public readonly Complex value;
        public bool IsReal => value.Imaginary == 0;

        public Number(Complex value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            if (value.Imaginary != 0 && !double.IsNaN(value.Real))
            {
                if (value.Real != 0)
                {
                    if (value.Imaginary == 1)
                        return $"{value.Real}+i";
                    else if (value.Imaginary == -1)
                        return $"{value.Real}-i";
                    else if (value.Imaginary > 0 && value.Imaginary != 1)
                        return $"{value.Real}+{value.Imaginary}i";
                    else
                        return $"{value.Real}{value.Imaginary}i";
                }
                else if (value.Imaginary == 1)
                    return $"i";
                else if (value.Imaginary == -1)
                    return $"-i";
                else
                    return $"{value.Imaginary}i";
            }
            return value.Real.ToString();
        }

        public override bool Equals(object obj)
        {
            Number num = obj as Number;
            if (num == null)
                return false;
            return value.Equals(num.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<Complex>.Default.GetHashCode(value);
        }
    }
}
