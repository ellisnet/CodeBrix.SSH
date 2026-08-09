using System;
using System.Buffers.Binary;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

public class SftpFileAttributesTest
{
    [Theory]
    [InlineData(0xC000u, true, false, false, false, false, false, false)] // Socket
    [InlineData(0xA000u, false, true, false, false, false, false, false)] // Symbolic link
    [InlineData(0x8000u, false, false, true, false, false, false, false)] // Regular file
    [InlineData(0x6000u, false, false, false, true, false, false, false)] // Block device
    [InlineData(0x4000u, false, false, false, false, true, false, false)] // Directory
    [InlineData(0x2000u, false, false, false, false, false, true, false)] // Character device
    [InlineData(0x1000u, false, false, false, false, false, false, true)] // Named pipe
    public void FileTypePropertiesAreMutuallyExclusive(
        uint permissions,
        bool isSocket,
        bool isSymbolicLink,
        bool isRegularFile,
        bool isBlockDevice,
        bool isDirectory,
        bool isCharacterDevice,
        bool isNamedPipe)
    {
        var attributeBytes = new byte[8];
        attributeBytes[3] = 0x4; // SSH_FILEXFER_ATTR_PERMISSIONS
        BinaryPrimitives.WriteUInt32BigEndian(attributeBytes.AsSpan(4), permissions);

        var attributes = SftpFileAttributes.FromBytes(attributeBytes);

        Assert.Equal(isSocket, attributes.IsSocket);
        Assert.Equal(isSymbolicLink, attributes.IsSymbolicLink);
        Assert.Equal(isRegularFile, attributes.IsRegularFile);
        Assert.Equal(isBlockDevice, attributes.IsBlockDevice);
        Assert.Equal(isDirectory, attributes.IsDirectory);
        Assert.Equal(isCharacterDevice, attributes.IsCharacterDevice);
        Assert.Equal(isNamedPipe, attributes.IsNamedPipe);
    }

    [Fact]
    public void FromBytesGetBytes()
    {
        // 81a4 in hex = 100644 in octal
        var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0x4, 0, 0, 0x81, 0xa4]);

        Assert.True(attributes.IsRegularFile);

        Assert.False(attributes.IsUIDBitSet);
        Assert.False(attributes.IsGroupIDBitSet);
        Assert.False(attributes.IsStickyBitSet);
        Assert.True(attributes.OwnerCanRead);
        Assert.True(attributes.OwnerCanWrite);
        Assert.False(attributes.OwnerCanExecute);
        Assert.True(attributes.GroupCanRead);
        Assert.False(attributes.GroupCanWrite);
        Assert.False(attributes.GroupCanExecute);
        Assert.True(attributes.OthersCanRead);
        Assert.False(attributes.OthersCanWrite);
        Assert.False(attributes.OthersCanExecute);

        Assert.Equal(-1, attributes.Size); // Erm, OK?
        Assert.Equal(-1, attributes.UserId);
        Assert.Equal(-1, attributes.GroupId);

        Assert.Equal(default, attributes.LastAccessTimeUtc);
        Assert.Equal(DateTimeKind.Utc, attributes.LastAccessTimeUtc.Kind);

        Assert.Equal(default, attributes.LastWriteTimeUtc);
        Assert.Equal(DateTimeKind.Utc, attributes.LastWriteTimeUtc.Kind);

        Assert.Equal("-rw-r--r--", attributes.ToString());

        // No changes
        Assert.Equal(
            new byte[] { 0, 0, 0, 0 },
            attributes.GetBytes());

        // Permissions change
        attributes.IsUIDBitSet = true;
        attributes.OwnerCanExecute = true;

        Assert.Equal(
            new byte[] { 0, 0, 0, 0x4, 0, 0, 0x89, 0xe4 },
            attributes.GetBytes());

        Assert.Equal("-rwsr--r--", attributes.ToString());

        // Size change
        attributes.Size = 123;

        Assert.Equal(
            new byte[] {
                0, 0, 0, 0x1 | 0x4,
                0, 0, 0, 0, 0, 0, 0, 123,
                0, 0, 0x89, 0xe4 },
            attributes.GetBytes());

        Assert.StartsWith("-rwsr--r-- Size: ", attributes.ToString(), StringComparison.Ordinal);

        // Uid/gid change
        attributes.UserId = 99;
        attributes.GroupId = 66;

        Assert.Equal(
            new byte[] {
                0, 0, 0, 0x1 | 0x2 | 0x4,
                0, 0, 0, 0, 0, 0, 0, 123,
                0, 0, 0, 99, 0, 0, 0, 66,
                0, 0, 0x89, 0xe4 },
            attributes.GetBytes());

        // Access/mod time change
        attributes.LastAccessTimeUtc = new DateTime(2025, 08, 10, 17, 51, 37, DateTimeKind.Unspecified);
        attributes.LastWriteTime = new DateTimeOffset(2016, 12, 02, 13, 18, 20, TimeSpan.FromHours(3)).LocalDateTime;

        var expectedTimeBytes = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(expectedTimeBytes, 1754848297);
        BinaryPrimitives.WriteUInt32BigEndian(expectedTimeBytes.AsSpan(4), 1480673900);

        Assert.Equal(
            new byte[] {
                0, 0, 0, 0x1 | 0x2 | 0x4 | 0x8,
                0, 0, 0, 0, 0, 0, 0, 123,
                0, 0, 0, 99, 0, 0, 0, 66,
                0, 0, 0x89, 0xe4
            }.Concat(expectedTimeBytes),
            attributes.GetBytes());

        Assert.Equal(new DateTime(2016, 12, 02, 10, 18, 20, DateTimeKind.Utc), attributes.LastWriteTimeUtc);
        Assert.Equal(DateTimeKind.Utc, attributes.LastWriteTimeUtc.Kind);

        var attributesString = attributes.ToString();
        Assert.StartsWith("-rwsr--r-- Size: ", attributesString, StringComparison.Ordinal);
        Assert.Contains(" LastWriteTime: ", attributesString, StringComparison.CurrentCulture);
    }

    [Theory]
    [InlineData((short)8888)]
    [InlineData((short)10000)]
    [InlineData((short)8000)]
    [InlineData((short)0080)]
    [InlineData((short)0008)]
    [InlineData((short)1797)]
    [InlineData((short)-1)]
    [InlineData(short.MaxValue)]
    public void SetPermissions_InvalidMode_ThrowsArgumentOutOfRangeException(short mode)
    {
        var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0]);

        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => attributes.SetPermissions(mode));
        Assert.Equal("mode", ex.ParamName);
    }

    [Theory]
    [InlineData((short)0777, false, false, false, true, true, true, true, true, true, true, true, true)]
    [InlineData((short)0755, false, false, false, true, true, true, true, false, true, true, false, true)]
    [InlineData((short)0644, false, false, false, true, true, false, true, false, false, true, false, false)]
    [InlineData((short)0444, false, false, false, true, false, false, true, false, false, true, false, false)]
    [InlineData((short)0000, false, false, false, false, false, false, false, false, false, false, false, false)]
    [InlineData((short)4700, true, false, false, true, true, true, false, false, false, false, false, false)]
    [InlineData((short)3001, false, true, true, false, false, false, false, false, false, false, false, true)]
    [InlineData((short)7777, true, true, true, true, true, true, true, true, true, true, true, true)]
    public void SetPermissions_ValidMode(
        short mode,
        bool setUid, bool setGid, bool sticky,
        bool ownerRead, bool ownerWrite, bool ownerExec,
        bool groupRead, bool groupWrite, bool groupExec,
        bool othersRead, bool othersWrite, bool othersExec)
    {
        var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0]);

        attributes.SetPermissions(mode);

        Assert.Equal(setUid, attributes.IsUIDBitSet);
        Assert.Equal(setGid, attributes.IsGroupIDBitSet);
        Assert.Equal(sticky, attributes.IsStickyBitSet);
        Assert.Equal(ownerRead, attributes.OwnerCanRead);
        Assert.Equal(ownerWrite, attributes.OwnerCanWrite);
        Assert.Equal(ownerExec, attributes.OwnerCanExecute);
        Assert.Equal(groupRead, attributes.GroupCanRead);
        Assert.Equal(groupWrite, attributes.GroupCanWrite);
        Assert.Equal(groupExec, attributes.GroupCanExecute);
        Assert.Equal(othersRead, attributes.OthersCanRead);
        Assert.Equal(othersWrite, attributes.OthersCanWrite);
        Assert.Equal(othersExec, attributes.OthersCanExecute);
    }

    [Theory]
    [InlineData(0xC000u, (short)1770, "srwxrwx--T")] // Socket
    [InlineData(0xA000u, (short)2707, "lrwx--Srwx")] // Symbolic link
    [InlineData(0x8000u, (short)4755, "-rwsr-xr-x")] // Regular file
    [InlineData(0x8000u, (short)4644, "-rwSr--r--")] // Regular file
    [InlineData(0x6000u, (short)2711, "brwx--s--x")] // Block device
    [InlineData(0x4000u, (short)1777, "drwxrwxrwt")] // Directory
    [InlineData(0x4000u, (short)1776, "drwxrwxrwT")] // Directory
    [InlineData(0x2000u, (short)0660, "crw-rw----")] // Character device
    [InlineData(0x1000u, (short)0022, "p----w--w-")] // Named pipe
    public void ToStringWithPermissions(
        uint fileType,
        short permissions,
        string expected)
    {
        var attributeBytes = new byte[8];
        attributeBytes[3] = 0x4; // SSH_FILEXFER_ATTR_PERMISSIONS
        BinaryPrimitives.WriteUInt32BigEndian(attributeBytes.AsSpan(4), fileType);

        var attributes = SftpFileAttributes.FromBytes(attributeBytes);

        attributes.SetPermissions(permissions);

        Assert.Equal(expected, attributes.ToString());
    }
}
