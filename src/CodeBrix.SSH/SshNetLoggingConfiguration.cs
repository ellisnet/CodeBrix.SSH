using CodeBrix.SSH.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeBrix.SSH; //was previously: Renci.SshNet;

/// <summary>
/// Allows configuring the logging for internal logs of CodeBrix.SSH.
/// </summary>
public static class SshNetLoggingConfiguration
{
    internal static ILoggerFactory LoggerFactory { get; private set; } = NullLoggerFactory.Instance;

    /// <summary>
    /// Initializes the logging for CodeBrix.SSH.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public static void InitializeLogging(ILoggerFactory loggerFactory)
    {
        ThrowHelper.ThrowIfNull(loggerFactory);
        LoggerFactory = loggerFactory;
    }

#if DEBUG
    /// <summary>
    /// Gets or sets the path to which to write session secrets which
    /// Wireshark can read and use to inspect encrypted traffic.
    /// </summary>
    /// <remarks>
    /// To configure in Wireshark, go to Edit -> Preferences -> Protocols
    /// -> SSH and set the same value for "Key log filename".
    /// </remarks>
    public static string WiresharkKeyLogFilePath { get; set; }
#endif
}
