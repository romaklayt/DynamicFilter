﻿// ReSharper disable MemberCanBePrivate.Global

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