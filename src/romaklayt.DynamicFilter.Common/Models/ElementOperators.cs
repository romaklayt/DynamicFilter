// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Common.Models;

internal static class ElementOperators
{
    public static Dictionary<string, ElementOperatorEnum> Get =>
        new()
        {
            { "==*", ElementOperatorEnum.EqualsCaseInsensitive },
            { "_-=*", ElementOperatorEnum.EndsWithCaseInsensitive },
            { "_=*", ElementOperatorEnum.StartsWithCaseInsensitive },
            { "@=*", ElementOperatorEnum.ContainsCaseInsensitive },
            { "_-=", ElementOperatorEnum.EndsWith },
            { "_=", ElementOperatorEnum.StartsWith },
            { "<=", ElementOperatorEnum.LessOrEqual },
            { ">=", ElementOperatorEnum.GreaterOrEqual },
            { "<", ElementOperatorEnum.LessThan },
            { ">", ElementOperatorEnum.GreaterThan },
            { "@=", ElementOperatorEnum.Contains },
            { "==", ElementOperatorEnum.Equals }
        };
}

internal enum ElementOperatorEnum
{
    Equals,
    Contains,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    StartsWith,
    EndsWith,
    ContainsCaseInsensitive,
    StartsWithCaseInsensitive,
    EndsWithCaseInsensitive,
    EqualsCaseInsensitive
}

internal static class EnumerableOperators
{
    public static Dictionary<string, EnumerableOperatorEnum> Get =>
        new()
        {
            { "any", EnumerableOperatorEnum.Any },
            { "all", EnumerableOperatorEnum.All }
        };
}

internal enum EnumerableOperatorEnum
{
    Any,
    All
}