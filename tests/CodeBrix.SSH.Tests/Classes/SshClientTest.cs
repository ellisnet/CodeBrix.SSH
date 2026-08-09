using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides client connection to SSH server.
/// </summary>
public class SshClientTest : TestBase
{
    [Fact]
    public void CreateShellStream1_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            const int bufferSize = 4096;

            try
            {
                client.CreateShellStream(terminalName, columns, rows, width, height, bufferSize);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShellStream2_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            var terminalModes = new Dictionary<TerminalModes, uint>();
            const int bufferSize = 4096;

            try
            {
                client.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, terminalModes);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell1_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var encoding = Encoding.UTF8;
            const string input = "INPUT";
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();

            try
            {
                client.CreateShell(encoding, input, output, extendedOutput);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell2_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var encoding = Encoding.UTF8;
            const string input = "INPUT";
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            var terminalModes = new Dictionary<TerminalModes, uint>();

            try
            {
                client.CreateShell(
                    encoding,
                    input,
                    output,
                    extendedOutput,
                    terminalName,
                    columns,
                    rows,
                    width,
                    height,
                    terminalModes);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell3_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var encoding = Encoding.UTF8;
            const string input = "INPUT";
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            var terminalModes = new Dictionary<TerminalModes, uint>();
            const int bufferSize = 4096;

            try
            {

                client.CreateShell(
                    encoding,
                    input,
                    output,
                    extendedOutput,
                    terminalName,
                    columns,
                    rows,
                    width,
                    height,
                    terminalModes,
                    bufferSize);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell4_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var input = new MemoryStream();
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();

            try
            {

                client.CreateShell(input, output, extendedOutput);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell5_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var input = new MemoryStream();
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            var terminalModes = new Dictionary<TerminalModes, uint>();

            try
            {
                client.CreateShell(
                    input,
                    output,
                    extendedOutput,
                    terminalName,
                    columns,
                    rows,
                    width,
                    height,
                    terminalModes);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateShell6_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var input = new MemoryStream();
            var output = new MemoryStream();
            var extendedOutput = new MemoryStream();
            const string terminalName = "vt100";
            const uint columns = 80;
            const uint rows = 25;
            const uint width = 640;
            const uint height = 480;
            var terminalModes = new Dictionary<TerminalModes, uint>();
            const int bufferSize = 4096;

            try
            {

                client.CreateShell(
                    input,
                    output,
                    extendedOutput,
                    terminalName,
                    columns,
                    rows,
                    width,
                    height,
                    terminalModes,
                    bufferSize);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateCommand_CommandText_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            try
            {
                client.CreateCommand("ls");
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void CreateCommand_CommandTextAndEncoding_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            try
            {
                client.CreateCommand("ls", Encoding.UTF8);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void AddForwardedPort_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            var port = new ForwardedPortLocal(50, "host", 8080);

            try
            {
                client.AddForwardedPort(port);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }

    [Fact]
    public void RunCommand_CommandText_NeverConnected()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
        {
            try
            {
                client.RunCommand("ls");
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SshConnectionException ex)
            {
                Assert.Null(ex.InnerException);
                Assert.Equal("Client not connected.", ex.Message);
            }
        }
    }
}
