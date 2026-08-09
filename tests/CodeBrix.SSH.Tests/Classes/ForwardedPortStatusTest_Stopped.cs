using Xunit;

#pragma warning disable SA1131 // Use readable conditions
namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortStatusTest_Stopped
{
    [Fact]
    public void ToStopping_ShouldReturnFalseAndNotChangeStatus()
    {
        var status = ForwardedPortStatus.Stopped;

        var actual = ForwardedPortStatus.ToStopping(ref status);

        Assert.False(actual);
        Assert.Equal(ForwardedPortStatus.Stopped, status);
    }

    [Fact]
    public void ToStarting_ShouldReturnTrueAndChangeStatusToStarting()
    {
        var status = ForwardedPortStatus.Stopped;

        var actual = ForwardedPortStatus.ToStarting(ref status);

        Assert.True(actual);
        Assert.Equal(ForwardedPortStatus.Starting, status);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNull()
    {
        const ForwardedPortStatus other = null;

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
    {
        var other = new object();

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnTrueWhenOtherIsStopped()
    {
        var other = ForwardedPortStatus.Stopped;

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.True(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStopping()
    {
        var other = ForwardedPortStatus.Stopping;

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStarted()
    {
        var other = ForwardedPortStatus.Started;

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStarting()
    {
        var other = ForwardedPortStatus.Starting;

        var actual = ForwardedPortStatus.Stopped.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Stopped;
        const ForwardedPortStatus right = null;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Stopped;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Stopped;

        var actual = left == right;

        Assert.True(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Stopping;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Started;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Starting;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Stopped;
        const ForwardedPortStatus right = null;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Stopped;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Stopped;

        var actual = left != right;

        Assert.False(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Stopping;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Started;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Stopped;
        var right = ForwardedPortStatus.Starting;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void GetHashCodeShouldReturnOne()
    {
        var actual = ForwardedPortStatus.Stopped.GetHashCode();

        Assert.Equal(1, actual);
    }

    [Fact]
    public void ToStringShouldReturnStopping()
    {
        var actual = ForwardedPortStatus.Stopped.ToString();

        Assert.Equal("Stopped", actual);
    }
}
