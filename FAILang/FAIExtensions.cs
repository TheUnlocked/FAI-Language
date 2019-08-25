using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAILang
{
    public static class FAIExtensions
    {
        public static (IEnumerable<T> matching, IEnumerable<T> unmatching) SplitBy<T>(this IEnumerable<T> enumerable, Func<T, bool> pred)
        {
            if (enumerable is ICollection<T>)
            {
                ICollection<T> matching = new List<T>();
                ICollection<T> nonMatching = new List<T>();
                foreach (T element in enumerable)
                {
                    if (pred(element))
                    {
                        matching.Add(element);
                    }
                    else
                    {
                        nonMatching.Add(element);
                    }
                }
                return (matching, nonMatching);
            }
            return (enumerable.Where(pred), enumerable.Where(x => !pred(x)));
        }
    }
}
