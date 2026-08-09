using System.Threading.Tasks;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public abstract class ChannelSessionTestBase : IAsyncLifetime
{
    internal Mock<ISession> SessionMock { get; private set; }
    internal Mock<IConnectionInfo> ConnectionInfoMock { get; private set; }

    // IAsyncLifetime rather than a constructor: Arrange and Act are virtual or
    // abstract, and running them from a base constructor would call the overrides
    // before the derived class's own field initializers have run.
    public virtual ValueTask InitializeAsync()
    {
        Arrange();
        Act();
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected abstract void SetupData();

    protected void CreateMocks()
    {
        SessionMock = new Mock<ISession>(MockBehavior.Strict);
        SessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        ConnectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
    }

    protected abstract void SetupMocks();

    protected virtual void Arrange()
    {
        SetupData();
        CreateMocks();
        SetupMocks();
    }

    protected abstract void Act();
}
