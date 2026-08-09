using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

/// <summary>
/// Marks a test that is only meaningful on one operating system. On any other
/// platform the test is skipped rather than run.
/// </summary>
/// <remarks>
/// Upstream implements this as an MSTest <c>TestMethodAttribute</c> subclass which
/// reports an inconclusive outcome off-platform. xUnit has no inconclusive outcome,
/// so this reports the test as skipped instead, which is the closest equivalent.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactForPlatformAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FactForPlatformAttribute"/> class.
    /// </summary>
    /// <param name="platform">
    /// The name of the platform the test is intended for, as accepted by
    /// <see cref="OSPlatform.Create(string)"/> — for example "Windows" or "Linux".
    /// </param>
    /// <param name="sourceFilePath">The source file the test is declared in. Supplied by the compiler.</param>
    /// <param name="sourceLineNumber">The line the test is declared on. Supplied by the compiler.</param>
    public FactForPlatformAttribute(
        string platform,
        [CallerFilePath] string sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        Platform = platform;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Create(platform)))
        {
            Skip = $"Test not executed. The test is intended for the '{platform}' platform only.";
        }
    }

    /// <summary>
    /// Gets the name of the platform the test is intended for.
    /// </summary>
    public string Platform { get; }
}
