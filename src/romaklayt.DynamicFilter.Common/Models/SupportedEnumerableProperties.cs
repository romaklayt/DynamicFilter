using System.Collections.Generic;
using System.Linq;

namespace romaklayt.DynamicFilter.Common.Models;

internal static class SupportedEnumerableProperties
{
    private const string Count = nameof(Count);

    private static string[] _value = [nameof(Enumerable.Count)];

    public static IEnumerable<string> Get => _value;
}