using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Connection; //was previously: Renci.SshNet.Tests.Classes.Messages.Connection;

/// <summary>
/// Represents "pty-req" type channel request information
/// </summary>
public class PseudoTerminalRequestInfoTest
{
    private string _environmentVariable;
    private uint _columns;
    private uint _rows;
    private uint _width;
    private uint _height;
    private IDictionary<TerminalModes, uint> _terminalModeValues;
    private byte[] _environmentVariableBytes;

    public PseudoTerminalRequestInfoTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _environmentVariable = random.Next().ToString(CultureInfo.InvariantCulture);
        _environmentVariableBytes = Encoding.UTF8.GetBytes(_environmentVariable);
        _columns = (uint)random.Next(0, int.MaxValue);
        _rows = (uint)random.Next(0, int.MaxValue);
        _width = (uint)random.Next(0, int.MaxValue);
        _height = (uint)random.Next(0, int.MaxValue);
        _terminalModeValues = new Dictionary<TerminalModes, uint>
        {
            {TerminalModes.CS8, 433},
            {TerminalModes.ECHO, 566}
        };
    }

    [Fact]
    public void GetBytes()
    {
        var target = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, _terminalModeValues);

        var bytes = target.GetBytes();

        var expectedBytesLength = 1; // WantReply
        expectedBytesLength += 4; // EnvironmentVariable length
        expectedBytesLength += _environmentVariableBytes.Length; // EnvironmentVariable
        expectedBytesLength += 4; // Columns
        expectedBytesLength += 4; // Rows
        expectedBytesLength += 4; // PixelWidth
        expectedBytesLength += 4; // PixelHeight
        expectedBytesLength += 4; // Length of "encoded terminal modes"
        expectedBytesLength += (_terminalModeValues.Count * (1 + 4)) + 1; // encoded terminal modes

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(1, sshDataStream.ReadByte()); // WantReply
        Assert.Equal(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
        Assert.Equal(_columns, sshDataStream.ReadUInt32());
        Assert.Equal(_rows, sshDataStream.ReadUInt32());
        Assert.Equal(_width, sshDataStream.ReadUInt32());
        Assert.Equal(_height, sshDataStream.ReadUInt32());
        Assert.Equal((uint)((_terminalModeValues.Count * (1 + 4)) + 1), sshDataStream.ReadUInt32());
        Assert.Equal((int)TerminalModes.CS8, sshDataStream.ReadByte());
        Assert.Equal(_terminalModeValues[TerminalModes.CS8], sshDataStream.ReadUInt32());
        Assert.Equal((int)TerminalModes.ECHO, sshDataStream.ReadByte());
        Assert.Equal(_terminalModeValues[TerminalModes.ECHO], sshDataStream.ReadUInt32());
        Assert.Equal((int)TerminalModes.TTY_OP_END, sshDataStream.ReadByte());

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void GetBytes_TerminalModeValues_Null()
    {
        var target = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, null);

        var bytes = target.GetBytes();

        var expectedBytesLength = 1; // WantReply
        expectedBytesLength += 4; // EnvironmentVariable length
        expectedBytesLength += _environmentVariableBytes.Length; // EnvironmentVariable
        expectedBytesLength += 4; // Columns
        expectedBytesLength += 4; // Rows
        expectedBytesLength += 4; // PixelWidth
        expectedBytesLength += 4; // PixelHeight
        expectedBytesLength += 4; // Length of "encoded terminal modes"

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(1, sshDataStream.ReadByte()); // WantReply
        Assert.Equal(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
        Assert.Equal(_columns, sshDataStream.ReadUInt32());
        Assert.Equal(_rows, sshDataStream.ReadUInt32());
        Assert.Equal(_width, sshDataStream.ReadUInt32());
        Assert.Equal(_height, sshDataStream.ReadUInt32());
        Assert.Equal((uint)0, sshDataStream.ReadUInt32());

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void GetBytes_TerminalModeValues_Empty()
    {
        var target = new PseudoTerminalRequestInfo(_environmentVariable,
                                                   _columns,
                                                   _rows,
                                                   _width,
                                                   _height,
                                                   new Dictionary<TerminalModes, uint>());

        var bytes = target.GetBytes();

        var expectedBytesLength = 1; // WantReply
        expectedBytesLength += 4; // EnvironmentVariable length
        expectedBytesLength += _environmentVariableBytes.Length; // EnvironmentVariable
        expectedBytesLength += 4; // Columns
        expectedBytesLength += 4; // Rows
        expectedBytesLength += 4; // PixelWidth
        expectedBytesLength += 4; // PixelHeight
        expectedBytesLength += 4; // Length of "encoded terminal modes"

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(1, sshDataStream.ReadByte()); // WantReply
        Assert.Equal(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
        Assert.Equal(_columns, sshDataStream.ReadUInt32());
        Assert.Equal(_rows, sshDataStream.ReadUInt32());
        Assert.Equal(_width, sshDataStream.ReadUInt32());
        Assert.Equal(_height, sshDataStream.ReadUInt32());
        Assert.Equal((uint)0, sshDataStream.ReadUInt32());

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void DefaultCtor()
    {
        var ptyReq = new PseudoTerminalRequestInfo();

        Assert.True(ptyReq.WantReply);
        Assert.Equal(uint.MinValue, ptyReq.Columns);
        Assert.Null(ptyReq.EnvironmentVariable);
        Assert.Equal("pty-req", ptyReq.RequestName);
        Assert.Equal(uint.MinValue, ptyReq.PixelHeight);
        Assert.Equal(uint.MinValue, ptyReq.PixelWidth);
        Assert.Equal(uint.MinValue, ptyReq.Rows);
        Assert.Null(ptyReq.TerminalModeValues);
    }

    [Fact]
    public void FullCtor()
    {
        var ptyReq = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, _terminalModeValues);

        Assert.True(ptyReq.WantReply);
        Assert.Equal(_columns, ptyReq.Columns);
        Assert.Same(_environmentVariable, ptyReq.EnvironmentVariable);
        Assert.Equal("pty-req", ptyReq.RequestName);
        Assert.Equal(_height, ptyReq.PixelHeight);
        Assert.Equal(_width, ptyReq.PixelWidth);
        Assert.Equal(_rows, ptyReq.Rows);
        Assert.Same(_terminalModeValues, ptyReq.TerminalModeValues);
    }
}
