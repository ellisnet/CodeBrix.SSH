using System.Threading.Tasks;
using Xunit;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

public abstract class TripleATestBase : IAsyncLifetime
{
    // IAsyncLifetime rather than a constructor: Arrange and Act are virtual, and
    // running them from a base constructor would call the overrides before the
    // derived class's own field initializers have run. This matches the ordering
    // of the [TestInitialize] method it replaces.
    public virtual ValueTask InitializeAsync()
    {
        Arrange();
        Act();
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        TearDown();
        return ValueTask.CompletedTask;
    }

    protected virtual void TearDown()
    {
    }

    protected abstract void Arrange();

    protected abstract void Act();
}
