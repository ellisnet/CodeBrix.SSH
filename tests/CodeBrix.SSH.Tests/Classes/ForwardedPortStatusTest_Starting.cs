using Xunit;

#pragma warning disable SA1131 // Use readable conditions
namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortStatusTest_Starting
{
    [Fact]
    public void ToStopping_ShouldReturnTrueAndChangeStatusToStopping()
    {
        var status = ForwardedPortStatus.Starting;

        var actual = ForwardedPortStatus.ToStopping(ref status);

        Assert.True(actual);
        Assert.Equal(ForwardedPortStatus.Stopping, status);
    }

    [Fact]
    public void ToStarting_ShouldReturnFalseAndNotChangeStatus()
    {
        var status = ForwardedPortStatus.Starting;

        var actual = ForwardedPortStatus.ToStarting(ref status);

        Assert.False(actual);
        Assert.Equal(ForwardedPortStatus.Starting, status);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNull()
    {
        const ForwardedPortStatus other = null;

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
    {
        var other = new object();

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStopped()
    {
        var other = ForwardedPortStatus.Stopped;

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStopping()
    {
        var other = ForwardedPortStatus.Stopping;

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseWhenOtherIsStarted()
    {
        var other = ForwardedPortStatus.Started;

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.False(actual);
    }

    [Fact]
    public void Equals_ShouldReturnTrueWhenOtherIsStarting()
    {
        var other = ForwardedPortStatus.Starting;

        var actual = ForwardedPortStatus.Starting.Equals(other);

        Assert.True(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Starting;
        const ForwardedPortStatus right = null;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Starting;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartingAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Stopped;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartingAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Stopping;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartingAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Started;

        var actual = left == right;

        Assert.False(actual);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrueWhenLeftIsStartingAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Starting;

        var actual = left == right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
    {
        var left = ForwardedPortStatus.Starting;
        const ForwardedPortStatus right = null;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
    {
        const ForwardedPortStatus left = null;
        var right = ForwardedPortStatus.Starting;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartingAndRightIsStopped()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Stopped;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartingAndRightIsStopping()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Stopping;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartingAndRightIsStarted()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Started;

        var actual = left != right;

        Assert.True(actual);
    }

    [Fact]
    public void InEqualityOperator_ShouldReturnFalseWhenLeftIsStartingAndRightIsStarting()
    {
        var left = ForwardedPortStatus.Starting;
        var right = ForwardedPortStatus.Starting;

        var actual = left != right;

        Assert.False(actual);
    }

    [Fact]
    public void GetHashCodeShouldReturnFour()
    {
        var actual = ForwardedPortStatus.Starting.GetHashCode();

        Assert.Equal(4, actual);
    }

    [Fact]
    public void ToStringShouldReturnStarting()
    {
        var actual = ForwardedPortStatus.Starting.ToString();

        Assert.Equal("Starting", actual);
    }
}
