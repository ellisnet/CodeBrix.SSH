using System;
using System.IO;
using System.Threading;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

/// <summary>
///This is a test class for SftpDownloadAsyncResultTest and is intended
///to contain all SftpDownloadAsyncResultTest Unit Tests
///</summary>
public class SftpDownloadAsyncResultTest : TestBase
{
    [Fact]
    public void SftpDownloadAsyncResultConstructorTest()
    {
        const AsyncCallback asyncCallback = null;
        var state = new object();
        var target = new SftpDownloadAsyncResult(asyncCallback, state);

        Assert.False(target.CompletedSynchronously);
        Assert.False(target.EndInvokeCalled);
        Assert.False(target.IsCompleted);
        Assert.False(target.IsDownloadCanceled);
        Assert.Equal(0UL, target.DownloadedBytes);
        Assert.Same(state, target.AsyncState);
    }

    [Fact]
    public void SetAsCompleted_Exception_CompletedSynchronously()
    {
        var downloadCompleted = new ManualResetEvent(false);
        object state = "STATE";
        Exception exception = new IOException();
        IAsyncResult callbackResult = null;
        var target = new SftpDownloadAsyncResult(asyncResult =>
            {
                downloadCompleted.Set();
                callbackResult = asyncResult;
            }, state);

        target.SetAsCompleted(exception, true);

        Assert.Same(target, callbackResult);
        Assert.False(target.IsDownloadCanceled);
        Assert.True(target.IsCompleted);
        Assert.True(target.CompletedSynchronously);
        Assert.True(downloadCompleted.WaitOne(TimeSpan.Zero));
    }

    [Fact]
    public void EndInvoke_CompletedWithException()
    {
        object state = "STATE";
        Exception exception = new IOException();
        var target = new SftpDownloadAsyncResult(null, state);
        target.SetAsCompleted(exception, true);

        try
        {
            target.EndInvoke();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (IOException ex)
        {
            Assert.Same(exception, ex);
        }
    }

    [Fact]
    public void Update()
    {
        var target = new SftpDownloadAsyncResult(null, null);

        target.Update(123);
        target.Update(431);

        Assert.Equal(431UL, target.DownloadedBytes);
    }
}
