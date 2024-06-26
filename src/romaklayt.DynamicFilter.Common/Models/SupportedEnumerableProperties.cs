using System.Collections.Generic;
using System.Linq;

namespace romaklayt.DynamicFilter.Common.Models;

internal static class SupportedEnumerableProperties
{
    private const string Count = nameof(Count);

    public static IEnumerable<string> GetOperators()
    {
        return new[] { nameof(Enumerable.Count) };
    }
}