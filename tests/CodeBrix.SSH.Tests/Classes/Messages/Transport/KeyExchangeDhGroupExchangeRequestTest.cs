using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Transport; //was previously: Renci.SshNet.Tests.Classes.Messages.Transport;

/// <summary>
/// Represents SSH_MSG_KEX_DH_GEX_REQUEST message.
/// </summary>
public class KeyExchangeDhGroupExchangeRequestTest
{
    private uint _minimum;
    private uint _preferred;
    private uint _maximum;

    private void Init()
    {
        var random = new Random();
        _minimum = (uint)random.Next(1, int.MaxValue);
        _preferred = (uint)random.Next(1, int.MaxValue);
        _maximum = (uint)random.Next(1, int.MaxValue);
    }

    [Fact]
    [Trait("Category", "KeyExchangeInitMessage")]
    [Trait("Owner", "olegkap")]
    [Trait("Description", "Validates KeyExchangeInitMessage message serialization.")]
    public void Test_KeyExchangeDhGroupExchangeRequest_GetBytes()
    {
        var request = new KeyExchangeDhGroupExchangeRequest(_minimum, _preferred, _maximum);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // Minimum
        expectedBytesLength += 4; // Preferred
        expectedBytesLength += 4; // Maximum

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(request.MessageNumber, sshDataStream.ReadByte());
        Assert.Equal(_minimum, sshDataStream.ReadUInt32());
        Assert.Equal(_preferred, sshDataStream.ReadUInt32());
        Assert.Equal(_maximum, sshDataStream.ReadUInt32());

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void Load()
    {
        var request = new KeyExchangeDhGroupExchangeRequest(_minimum, _preferred, _maximum);
        var bytes = request.GetBytes();
        var target = new KeyExchangeDhGroupExchangeRequest(0, 0, 0);

        target.Load(bytes, 1, bytes.Length - 1);

        Assert.Equal(_minimum, target.Minimum);
        Assert.Equal(_preferred, target.Preferred);
        Assert.Equal(_maximum, target.Maximum);
    }
}
