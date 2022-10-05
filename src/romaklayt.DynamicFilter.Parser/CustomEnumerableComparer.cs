using System;
using System.Collections.Generic;
using System.Linq;

namespace romaklayt.DynamicFilter.Parser;

internal class CustomEnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
    where T : IComparable<T>
{
    public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
    {
        if (object.Equals(first, second))
            return true;
        if (first == null || second == null)
            return false;

        return new HashSet<T>(first).SetEquals(second);
    }

    public int GetHashCode(IEnumerable<T> enumerable) =>
        enumerable.OrderBy(x => x)
            .Aggregate(17, (current, val) => current * 23 + val.GetHashCode());
}