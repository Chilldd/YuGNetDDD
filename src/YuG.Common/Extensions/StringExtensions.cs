using System.Text.RegularExpressions;

namespace YuG.Common.Extensions;

public static partial class StringExtensions
{
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) =>
        string.IsNullOrEmpty(value);

    public static string Truncate(this string value, int maxLength)
    {
        Guard.AgainstNull(value, nameof(value));
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public static string ToSnakeCase(this string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        return SnakeCaseRegex().Replace(value, "_$1").ToLowerInvariant().TrimStart('_');
    }

    public static string ToKebabCase(this string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        return KebabCaseRegex().Replace(value, "-$1").ToLowerInvariant().TrimStart('-');
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SnakeCaseRegex();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex KebabCaseRegex();
}
