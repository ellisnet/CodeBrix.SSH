using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

internal static class ThrowHelper
{
    public static void ThrowObjectDisposedIf(bool condition, object instance)
    {
        ObjectDisposedException.ThrowIf(condition, instance);
    }

    public static void ThrowIfNull([NotNull] object argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
    }

    public static void ThrowIfNullOrWhiteSpace([NotNull] string argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
    }

    public static void ThrowIfNullOrEmpty([NotNull] string argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
    }

    public static void ThrowIfNegative(long value, [CallerArgumentExpression(nameof(value))] string paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
    }
}
