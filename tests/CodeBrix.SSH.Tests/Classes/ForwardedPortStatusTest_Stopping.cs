using System;
using Xunit;

#pragma warning disable SA1131 // Use readable conditions
namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortStatusTest_Stopping
{
    [Fact]
    public void ToStopping_ShouldReturnFalseAndNotChangeStatus()
    {
        var status = ForwardedPortStatus.Stopping;

        var actual = ForwardedPortStatus.ToStopping(ref status);

        Assert.False(actual);
        Assert.Equal(ForwardedPortStatus.Stopping, status);
    }

    [Fact]
    public void ToStarting_ShouldThrowInvalidOperationExceptionAndNotChangeStatus()
    {
        var status = ForwardedPortStatus.Stopping;

        try
        {
            ForwardedPortStatus.ToStarting(ref status);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(
                string.Format("Forwarded port cannot transition from '{0}' to '{1}'.",
                              ForwardedPortStatus.Stopping,
                              ForwardedPortStatus.Starting),
                ex.Message);
        }

        Assert.Equal(ForwardedPortStatus.Stopping, status);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNull()
    {
        const ForwardedPortStatus other = null;

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
    {
        var other = new object();

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStopped()
    {
        var other = ForwardedPortStatus.Stopped;

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnTrueWhenOtherIsStopping()
    {
        var other = ForwardedPortStatus.Stopping;

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.True(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStarted()
    {
        var other = ForwardedPortStatus.Started;

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStarting()
    {
        var other = ForwardedPortStatus.Starting;

        var actual = ForwardedPortStatus.Stopping.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Stopping;
        const ForwardedPortStatus right = null;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Stopping;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Stopped;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Stopping;

        var actual = left == right;

        Assert.True(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Started;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Starting;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Stopping;
        const ForwardedPortStatus right = null;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Stopping;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Stopped;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Stopping;

        var actual = left != right;

        Assert.False(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Started;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Stopping;
        var right = ForwardedPortStatus.Starting;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void GetHashCodeShouldReturnTwo()
    {
        var actual = ForwardedPortStatus.Stopping.GetHashCode();

        Assert.Equal(2, actual);
    }

    [Fact]
    public void ToStringShouldReturnStopping()
    {
        var actual = ForwardedPortStatus.Stopping.ToString();

        Assert.Equal("Stopping", actual);
    }
}
