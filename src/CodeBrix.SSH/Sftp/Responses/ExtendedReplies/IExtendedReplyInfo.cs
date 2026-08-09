using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.Sftp.Responses; //was previously: Renci.SshNet.Sftp.Responses;

/// <summary>
/// Extended Reply Info.
/// </summary>
internal interface IExtendedReplyInfo
{
    /// <summary>
    /// Loads the data from the stream into the instance.
    /// </summary>
    /// <param name="stream">The stream.</param>
    void LoadData(SshDataStream stream);
}
