// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Common.Models;

internal class FilterArrayLogicOperators
{
    public const string Or = "||";
    public const string And = "&&";

    public static string[] GetOperators() => [Or, And];
}

internal enum FilterArrayLogicOperatorEnum
{
    Or,
    And
}

internal class FilterEnumerableLogicOperators
{
    public const char Or = '|';
    public const char And = '&';

    public static Dictionary<char, FilterArrayLogicOperatorEnum> Get =>
        new()
        {
            { Or, FilterArrayLogicOperatorEnum.Or },
            { And, FilterArrayLogicOperatorEnum.And }
        };
}