using System;
using System.Collections.Generic;
using System.Text;

namespace FAILang.Types
{
    interface IIndexable
    {
        int Length { get; }
        IType IndexRange(int left_b, int right_b);
        IType Index(int index);
    }
}
