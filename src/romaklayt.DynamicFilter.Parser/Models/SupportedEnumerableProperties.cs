using System.Collections.Generic;
using System.Linq;

namespace romaklayt.DynamicFilter.Parser.Models;

internal static class SupportedEnumerableProperties
{
    private const string Count = nameof(Count);

    internal static IEnumerable<string> GetOperators()
    {
        return new[]
        {
            Count
        }.Select(s => s.ToLower());
    }
}