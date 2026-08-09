using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ScpClientTest_Download_PathAndFileInfo_SendExecRequestReturnsFalse : ScpClientTestBase
{
    private ConnectionInfo _connectionInfo;
    private ScpClient _scpClient;
    private FileInfo _fileInfo;
    private string _path;
    private string _transformedPath;
    private IList<ScpUploadEventArgs> _uploadingRegister;
    private SshException _actualException;

    protected override void SetupData()
    {
        var random = new Random();

        _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
        _fileInfo = new FileInfo("destination");
        _path = "/home/sshnet/" + random.Next().ToString(CultureInfo.InvariantCulture);
        _transformedPath = random.Next().ToString();
        _uploadingRegister = new List<ScpUploadEventArgs>();
    }

    protected override void SetupMocks()
    {
        var sequence = new MockSequence();

        ServiceFactoryMock.InSequence(sequence)
                           .Setup(p => p.CreateRemotePathDoubleQuoteTransformation())
                           .Returns(_remotePathTransformationMock.Object);
        ServiceFactoryMock.InSequence(sequence)
                           .Setup(p => p.CreateSocketFactory())
                           .Returns(SocketFactoryMock.Object);
        ServiceFactoryMock.InSequence(sequence)
                           .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                           .Returns(SessionMock.Object);
        SessionMock.InSequence(sequence).Setup(p => p.Connect());
        ServiceFactoryMock.InSequence(sequence).Setup(p => p.CreatePipeStream()).Returns(_pipeStreamMock.Object);
        SessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
        _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
        _remotePathTransformationMock.InSequence(sequence)
                                     .Setup(p => p.Transform(_path))
                                     .Returns(_transformedPath);
        _channelSessionMock.InSequence(sequence)
            .Setup(p => p.SendExecRequest(string.Format("scp -pf {0}", _transformedPath))).Returns(false);
        _channelSessionMock.InSequence(sequence).Setup(p => p.Dispose());
        _pipeStreamMock.InSequence(sequence).Setup(p => p.Close());
    }

    protected override void Arrange()
    {
        base.Arrange();

        _scpClient = new ScpClient(_connectionInfo, false, ServiceFactoryMock.Object);
        _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
        _scpClient.Connect();
    }

    protected override void Act()
    {
        try
        {
            _scpClient.Download(_path, _fileInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void UploadShouldHaveThrownSshException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal("Secure copy execution request was rejected by the server. Please consult the server logs.", _actualException.Message);
    }

    [Fact]
    public void SendExecRequestOnChannelSessionShouldBeInvokedOnce()
    {
        _channelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -pf {0}", _transformedPath)), Times.Once);
    }

    [Fact]
    public void DisposeOnChannelShouldBeInvokedOnce()
    {
        _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void DisposeOnPipeStreamShouldBeInvokedOnce()
    {
        _pipeStreamMock.Verify(p => p.Close(), Times.Once);
    }

    [Fact]
    public void UploadingShouldNeverHaveFired()
    {
        Assert.Empty(_uploadingRegister);
    }
}
