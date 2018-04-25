using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    public struct Error : IType
    {
        public string TypeName => "Error";

        public string name;
        public string message;
        public Error(string name, string message)
        {
            this.name = name;
            this.message = message;
        }

        public override string ToString()
        {
            return $"{name}: {message}";
        }

        public override int GetHashCode()
        {
            int hash = 691949981;
            hash = hash * 1532528149 + name.GetHashCode();
            hash = hash * 1532528149 + message.GetHashCode();
            return hash;
        }
    }
}
