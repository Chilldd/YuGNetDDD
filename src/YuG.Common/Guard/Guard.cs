namespace YuG.Common.Guarding;

public static class Guard
{
    public static T AgainstNull<T>([NotNull] T? value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        return value;
    }

    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value;
    }

    public static string AgainstNullOrEmpty(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
        return value;
    }

    public static int AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than zero.");
        return value;
    }

    public static long AgainstNegativeOrZero(long value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than zero.");
        return value;
    }

    public static decimal AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than zero.");
        return value;
    }

    public static Guid AgainstEmptyGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{parameterName} cannot be empty GUID.", parameterName);
        return value;
    }
}
