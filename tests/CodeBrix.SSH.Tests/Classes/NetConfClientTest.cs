using System;
using System.Threading.Tasks;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class NetConfClientTest : TestBase
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        SetUp();
    }

    private Random _random;

    private void SetUp()
    {
        _random = new Random();
    }

    [Fact]
    public void OperationTimeout_Default()
    {
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo);

        var actual = target.OperationTimeout;

        Assert.Equal(TimeSpan.FromMilliseconds(-1), actual);
    }

    [Fact]
    public void OperationTimeout_InsideLimits()
    {
        var operationTimeout = TimeSpan.FromMilliseconds(_random.Next(0, int.MaxValue - 1));
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo)
        {
            OperationTimeout = operationTimeout
        };

        var actual = target.OperationTimeout;

        Assert.Equal(operationTimeout, actual);
    }

    [Fact]
    public void OperationTimeout_LowerLimit()
    {
        var operationTimeout = TimeSpan.FromMilliseconds(-1);
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo)
        {
            OperationTimeout = operationTimeout
        };

        var actual = target.OperationTimeout;

        Assert.Equal(operationTimeout, actual);
    }

    [Fact]
    public void OperationTimeout_UpperLimit()
    {
        var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo)
        {
            OperationTimeout = operationTimeout
        };

        var actual = target.OperationTimeout;

        Assert.Equal(operationTimeout, actual);
    }

    [Fact]
    public void OperationTimeout_LessThanLowerLimit()
    {
        var operationTimeout = TimeSpan.FromMilliseconds(-2);
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo);

        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => target.OperationTimeout = operationTimeout);
        Assert.Equal("OperationTimeout", ex.ParamName);
    }

    [Fact]
    public void OperationTimeout_GreaterThanLowerLimit()
    {
        var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue).Add(TimeSpan.FromMilliseconds(1));
        var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
        var target = new NetConfClient(connectionInfo);

        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => target.OperationTimeout = operationTimeout);
        Assert.Equal("OperationTimeout", ex.ParamName);
    }
}
