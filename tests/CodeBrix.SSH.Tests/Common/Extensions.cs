using System.Collections.Generic;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

internal static class Extensions
{
    public static string AsString(this IList<ExceptionEventArgs> exceptionEvents)
    {
        if (exceptionEvents.Count == 0)
        {
            return string.Empty;
        }

        var reportedExceptions = string.Empty;
        foreach (var exceptionEvent in exceptionEvents)
        {
            reportedExceptions += exceptionEvent.Exception.ToString();
        }

        return reportedExceptions;
    }
}
