using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAILang.Types
{
    public class Error : IType
    {
        public virtual string TypeName => "Error";

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
    }
}
