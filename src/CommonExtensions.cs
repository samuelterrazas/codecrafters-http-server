using System.ComponentModel;
using System.Reflection;

namespace codecrafters_http_server;

internal static class CommonExtensions
{
    public static string GetEnumDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var description = value
            .GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>();

        return description?.Description ?? value.ToString();
    }
}