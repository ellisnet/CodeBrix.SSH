using CodeBrix.SSH.NetConf;
using CodeBrix.TestMocks.Mocking;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public abstract class NetConfClientTestBase : BaseClientTestBase
{
    internal Mock<INetConfSession> NetConfSessionMock { get; private set; }

    protected override void CreateMocks()
    {
        base.CreateMocks();

        NetConfSessionMock = new Mock<INetConfSession>(MockBehavior.Strict);
    }
}
