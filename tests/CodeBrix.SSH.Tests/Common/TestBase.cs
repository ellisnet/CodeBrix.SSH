using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

public abstract class TestBase : IAsyncLifetime
{
    private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

    // IAsyncLifetime rather than a constructor: OnInit is virtual, so running it
    // from a base constructor would call the override before the derived class's
    // own field initializers have run.
    public virtual ValueTask InitializeAsync()
    {
        OnInit();
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        OnCleanup();
        return ValueTask.CompletedTask;
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnCleanup()
    {
    }

    /// <summary>
    /// Creates the test file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="size">Size in megabytes.</param>
    protected void CreateTestFile(string fileName, int size)
    {
        using (var testFile = File.Create(fileName))
        {
            var random = new Random();
            for (int i = 0; i < 1024 * size; i++)
            {
                var buffer = new byte[1024];
                random.NextBytes(buffer);
                testFile.Write(buffer, 0, buffer.Length);
            }
        }
    }

    internal static Stream GetData(string name)
    {
        string resourceName = $"CodeBrix.SSH.Tests.Data.{name}";

        return ExecutingAssembly.GetManifestResourceStream(resourceName)
            ?? throw new ArgumentException($"Resource '{resourceName}' not found in assembly '{typeof(TestBase).Assembly.FullName}'.");
    }
}
