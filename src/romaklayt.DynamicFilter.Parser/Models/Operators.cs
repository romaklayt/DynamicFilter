namespace romaklayt.DynamicFilter.Parser.Models;

internal class Operators
{
    public new const string Equals = "==";
    public const string Contains = "@=";
    public const string GreaterThan = ">";
    public const string LessThan = "<";
    public const string GreaterOrEqual = ">=";
    public const string LessOrEqual = "<=";
    public const string StartsWith = "_=";
    public const string EndsWith = "_-=";
    public const string ContainsCaseInsensitive = "@=*";
    public const string StartsWithCaseInsensitive = "_=*";
    public const string EndsWithCaseInsensitive = "_-=*";
    public const string EqualsCaseInsensitive = "==*";

    internal static string[] GetOperators()
    {
        return new[]
        {
            Equals, Contains, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual, StartsWith,
            ContainsCaseInsensitive,
            StartsWithCaseInsensitive, EndsWithCaseInsensitive, EqualsCaseInsensitive
        };
    }
}