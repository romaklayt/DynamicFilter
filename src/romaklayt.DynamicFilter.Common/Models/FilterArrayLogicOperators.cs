// ReSharper disable MemberCanBePrivate.Global

namespace romaklayt.DynamicFilter.Common.Models;

public class FilterArrayLogicOperators
{
    public const string Or = "||";
    public const string And = "&&";

    public static string[] GetOperators()
    {
        return new[]
        {
            Or, And
        };
    }
}

public enum FilterArrayLogicOperatorEnum
{
    Or,
    And
}