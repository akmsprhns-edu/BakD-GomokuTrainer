using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib.Extensions
{
    public static class LinqExtensions
    {
       public static IEnumerable<TResult> ZipThree<T1, T2, T3, TResult>(
           this IEnumerable<T1> first,
           IEnumerable<T2> second,
           IEnumerable<T3> third,
           Func<T1, T2, T3, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (third == null) throw new ArgumentNullException(nameof(second));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            using var e1 = first.GetEnumerator();
            using var e2 = second.GetEnumerator();
            using var e3 = third.GetEnumerator();
            while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                yield return resultSelector(e1.Current, e2.Current, e3.Current);
        }
    }
}
