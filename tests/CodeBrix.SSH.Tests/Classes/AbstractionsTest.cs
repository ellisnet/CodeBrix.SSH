using System;
using System.Threading;
using CodeBrix.SSH.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class AbstractionsTest
{
    [Fact]
    public void CryptoAbstraction_GenerateRandom_ShouldPerformNoOpWhenDataIsZeroLength()
    {
        Assert.Empty(CryptoAbstraction.GenerateRandom(0));
    }

    [Fact]
    public void CryptoAbstraction_GenerateRandom_ShouldGenerateRandomSequenceOfValues()
    {
        var dataLength = new Random().Next(1, 100);

        var dataA = CryptoAbstraction.GenerateRandom(dataLength);
        var dataB = CryptoAbstraction.GenerateRandom(dataLength);

        Assert.Equal(dataLength, dataA.Length);
        Assert.Equal(dataLength, dataB.Length);

        Assert.NotEqual(dataA, dataB);
    }

    [Fact]
    public void ThreadAbstraction_ExecuteThread_ShouldThrowArgumentNullExceptionWhenActionIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => ThreadAbstraction.ExecuteThread(null));

        Assert.Null(ex.InnerException);
        Assert.Equal("action", ex.ParamName);
    }

    [Fact]
    public void ThreadAbstraction_ExecuteThread_ShouldExecuteActionOnSeparateThread()
    {
        int threadId = 0;
        using var waitHandle = new ManualResetEventSlim();

        ThreadAbstraction.ExecuteThread(() =>
        {
            threadId = Environment.CurrentManagedThreadId;
            waitHandle.Set();
        });

        Assert.True(waitHandle.Wait(1000, TestContext.Current.CancellationToken));
        Assert.NotEqual(0, threadId);
        Assert.NotEqual(Environment.CurrentManagedThreadId, threadId);
    }
}
