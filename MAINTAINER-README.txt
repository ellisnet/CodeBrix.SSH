================================================================================
MAINTAINER-README: CodeBrix.SSH
Notes for people and agents MAINTAINING this repository — not for package
consumers
================================================================================

Consumers of the NuGet package should read AGENT-README.txt instead. This file
covers building, testing, packaging and the provenance of the vendored source.


PURPOSE AND SCOPE
================================================================================

This repository produces exactly one NuGet package:

    CodeBrix.SSH.MitLicenseForever
        Assembly / root namespace: CodeBrix.SSH
        Project:  src/CodeBrix.SSH/CodeBrix.SSH.csproj
        License:  MIT
        Consumer documentation: AGENT-README.txt (repo root)

There are no sibling packages, no samples and no tools in this repository. The
only non-package project is the unit test project.


REPOSITORY LAYOUT
================================================================================

    CodeBrix.SSH.slnx           Solution. Solution Items folder carries
                                AGENT-README.txt, icon-codebrix-128.png,
                                LICENSE, README.md and THIRD-PARTY-NOTICES.txt;
                                the Tests folder carries the test project.

    src/CodeBrix.SSH/
        *.cs                    Public entry points at the project root:
                                SshClient, SftpClient, ScpClient,
                                NetConfClient, BaseClient, PrivateKeyFile,
                                ConnectionInfo and subclasses, the
                                authentication methods, the forwarded ports,
                                Session, Shell, ShellStream, SshCommand.
        Abstractions/           Internal platform and crypto abstractions.
                                No public types.
        Channels/               SSH channel implementations (session,
                                direct-tcpip, forwarded-tcpip, x11). No public
                                types.
        Common/                 Exceptions, event args, extension methods,
                                buffered streams, TerminalModes, the ASN.1/DER
                                readers.
        Compression/            zlib and zlib@openssh.com.
        Connection/             Direct, HTTP-proxy and SOCKS connectors,
                                protocol version exchange.
        KnownHosts/             New in this fork: KnownHostsStore,
                                HostKeyVerificationResult, HostKeyMismatchInfo,
                                KnownHostsClientExtensions.
        Messages/               SSH wire protocol messages, grouped into
                                Authentication/, Connection/ and Transport/.
        Netconf/                NETCONF-over-SSH subsystem session (folder
                                spelling differs from the CodeBrix.SSH.NetConf
                                namespace it declares).
        Security/               Key exchange algorithms, host keys, key
                                implementations, ciphers, MACs, and the
                                Cryptography/ sub-tree.
        Sftp/                   SFTP session, file, attribute and stream types,
                                with Requests/ and Responses/ holding the SFTP
                                wire protocol messages.
        Properties/             Assembly-level CLSCompliant declaration.
        InternalsVisibleTo.cs   Grants internal access to CodeBrix.SSH.Tests
                                and to DynamicProxyGenAssembly2 (needed by
                                CodeBrix.TestMocks to proxy internal
                                interfaces). Do not remove either line.

    tests/CodeBrix.SSH.Tests/
        Classes/                Mirrors the source layout.
        Common/                 Shared test helpers and base classes
                                (AsyncSocketListener, TestBase,
                                TripleATestBase, FactForPlatformAttribute,
                                SftpFileAttributesBuilder, assertion helpers).
        Data/                   Embedded key, certificate and sample data
                                fixtures, included as <EmbeddedResource>.
        Properties/             AssemblyInfo.cs (parallelisation off,
                                ExcludeFromCodeCoverage) plus the hand-written
                                Resources.Designer.cs and Resources.resx.


BUILDING
================================================================================

    dotnet restore CodeBrix.SSH.slnx
    dotnet build   CodeBrix.SSH.slnx

Requirements: the .NET 10 SDK. Nothing else -- no native toolchain, no Docker,
no SSH server. There is no global.json, so the newest installed .NET 10 SDK is
used.

The build must be 0 warnings / 0 errors. <GenerateDocumentationFile> is true,
so every public and protected member needs an XML doc comment; fix CS1591 by
writing the documentation, never by adding <NoWarn>, <WarningLevel> or
#pragma warning disable.

GeneratePackageOnBuild is true, so a plain `dotnet build` of the library also
produces a .nupkg.


TESTING
================================================================================

    dotnet test CodeBrix.SSH.slnx

The suite is a port of the upstream SSH.NET unit tests to xUnit.v3 and is
entirely self-contained: it runs offline, needs no SSH server and needs no
Docker. The upstream integration tests (which require Testcontainers and a
live sshd) and the benchmark projects are deliberately not part of this
repository.

Test dependencies: xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk,
CodeBrix.TestMocks.ApacheLicenseForever and
SilverAssertions.ApacheLicenseForever.

MOCKING USES CodeBrix.TestMocks, NOT Moq. CodeBrix.TestMocks is a fork of Moq
with the same API under the CodeBrix.TestMocks.Mocking namespace -- Mock<T>,
It, Times, MockBehavior all behave identically, so upstream test code carries
over with only the using directive changed. Do NOT add a Moq package
reference.

Tests exercise internal types directly, which works because
InternalsVisibleTo.cs grants access to CodeBrix.SSH.Tests. CodeBrix.TestMocks
mocks internal interfaces such as ISession and ISftpSession, and its
DynamicProxy fork emits proxies into an assembly named DynamicProxyGenAssembly2
-- which is why InternalsVisibleTo.cs also grants access to that name.

Test parallelisation is disabled for the whole assembly, in
tests/CodeBrix.SSH.Tests/Properties/AssemblyInfo.cs. Many of these tests bind
listeners to fixed local ports, so running collections concurrently makes them
fail with "Address already in use". Do not remove that attribute.

Any call inside a test that accepts a CancellationToken must be passed
TestContext.Current.CancellationToken, or xUnit1051 fires.

KNOWN FLAKE -- `dotnet test` occasionally exits non-zero while still reporting
every test as passing, with a line like:

    [xUnit.net] Catastrophic failure: System.Net.Sockets.SocketException : Broken pipe

The socket-based tests tear down real sockets on background threads, and a
peer can drop a connection at just the wrong moment. MSTest ignored exceptions
raised on non-test threads; xUnit installs a global unhandled-exception hook
and treats them as a fatal run error, even though no test failed. The
exception varies between runs (Broken pipe, ObjectDisposedException) and no
stack trace reaches the VSTest adapter.

It has only ever been observed through the VSTest bridge. Running the test
assembly directly with the native xUnit v3 runner has been consistently clean:

    tests/CodeBrix.SSH.Tests/bin/Debug/net10.0/CodeBrix.SSH.Tests

Prefer that command when a reliable exit code matters.


PACKAGING AND PUBLISHING
================================================================================

PackageId is CodeBrix.SSH.MitLicenseForever; the ".MitLicenseForever" suffix
belongs to the package id only, never to the assembly name, root namespace or
any type name.

VERSIONING. The version is date-stamped and computed by MSBuild at build time
from System.DateTime.UtcNow, in the form

    1.<whole years since the base year>.<day of year>.<minute of day>

with the base year set by the _VersionBaseYear property in the csproj. The
value is strictly increasing over time and is NOT SemVer -- major is pinned to
1 and minor encodes the year, so neither signals API compatibility. Do not add
a literal <Version> to the csproj. Two builds within the same UTC minute
produce the same version, so do not publish two packages from within one
minute.

WHAT SHIPS IN THE NUPKG, besides the assembly and its XML documentation file:

    README.md                  (PackageReadmeFile)
    icon-codebrix-128.png      (PackageIcon)
    AGENT-README.txt           consumer documentation
    THIRD-PARTY-NOTICES.txt    upstream licences and the modification log

MAINTAINER-README.txt, EXTRAS-README.txt and README-INDEX.txt are repository
files and are not packed.

PackageRequireLicenseAcceptance is true and PackageLicenseExpression is MIT.
GeneratePackageOnBuild is true, so every build of the library emits a fresh
.nupkg with a fresh version.


PROVENANCE AND VENDORED SOURCES
================================================================================

THIRD-PARTY-NOTICES.txt is the authoritative record; it reproduces both
upstream licences in full and lists every modification. Summary:

  * Substantially all source in src/ and tests/ is derived from SSH.NET at tag
    2025.1.0 (https://github.com/sshnet/SSH.NET), MIT licensed. One file within
    SSH.NET is in turn derived from a separately-licensed BCrypt
    implementation, also recorded in THIRD-PARTY-NOTICES.txt.
  * Six individually selected post-2025.1.0 upstream commits are applied on
    top: the ScpClient.Download truncation fix, ScpClient.UseDirectoryFlag, the
    SftpClient.UploadFileAsync canOverride overload, the IProgress overloads on
    DownloadFileAsync/UploadFileAsync (adding DownloadFileProgressReport and
    UploadFileProgressReport), the thread-pool-callback restoration that goes
    with them, and a documentation fix. Upstream's unreleased packet I/O and
    cipher performance rework was deliberately NOT taken.
  * Namespace rename: Renci.SshNet -> CodeBrix.SSH, one-to-one for every
    sub-namespace. Public type names are unchanged.
  * Target framework narrowed to net10.0, with all framework-selection
    #if/#else/#endif blocks resolved and removed (#if DEBUG left intact).
    Several upstream files that existed only to serve other target frameworks
    are absent.
  * Nullable reference type annotations removed; block-scoped namespaces
    converted to file-scoped; usings consolidated.
  * Versioning changed from Nerdbank.GitVersioning to the CodeBrix date-stamped
    scheme, so Session reads its version from
    AssemblyInformationalVersionAttribute at runtime. The protocol
    version-exchange string is therefore SSH-2.0-CodeBrix.SSH.SshClient.<version>.
  * Strong-name signing removed; InternalsVisibleTo declarations rewritten
    without public keys.
  * Cryptography dependency swapped from BouncyCastle.Cryptography to
    CodeBrix.Cryptography.MitLicenseForever (the CodeBrix fork of
    BouncyCastle.NET). Only the namespace root changed in the source. No
    BouncyCastle package may be reintroduced anywhere in the dependency graph.
  * Tests migrated from MSTest to xUnit.v3 and from Moq to CodeBrix.TestMocks,
    keeping upstream class and method names.

FILE-LEVEL PROVENANCE. Every .cs file ported from SSH.NET carries a provenance
comment on its namespace line:

    namespace CodeBrix.SSH.Sftp; //was previously: Renci.SshNet.Sftp;

Keep it when editing a ported file. Files that are new in this fork (the
KnownHosts/ folder, DownloadFileProgressReport.cs, UploadFileProgressReport.cs)
do not get one.

WHEN TAKING FURTHER UPSTREAM CHANGES: adapt each commit to this fork's
namespace, file layout and nullable conventions, add it to the "Post-2025.1.0
commits" list in THIRD-PARTY-NOTICES.txt with its short hash and a one-line
description, and reflect any consumer-visible change in AGENT-README.txt.


CODING CONVENTIONS
================================================================================

These rules apply to every change made to this repository.

  * Target framework is net10.0 only. Do not add target frameworks and do not
    reintroduce framework-selection #if directives.

  * Nullable reference types are OFF. Never write `?` on a reference type
    (`string?`, `MyClass?`, `object?`) and never use the null-forgiveness `!`
    operator. Value-type nullables (`int?`, `bool?`, `DateTime?`) are fine.
    Do not add `#nullable` directives or a <Nullable> csproj property.

  * No implicit usings and no `global using`. Every file declares the usings
    it needs.

  * File-scoped namespaces only (`namespace X;`), never block-scoped.

  * Usings sit above the namespace in a single contiguous block with no blank
    lines inside it: System.* first, then everything else, alphabetical within
    each group, with aliases last.

  * Every public and protected member needs an XML doc comment.

  * The build must be 0 warnings / 0 errors. Do not add <NoWarn>,
    <WarningLevel> or #pragma warning disable to silence anything. (Pragmas
    inherited from upstream that document a deliberate analyzer exception may
    stay.)

  * Tests use xUnit.v3 with CodeBrix.TestMocks for mocking and SilverAssertions
    where an assertion needs to carry a diagnostic message. Do NOT add Moq.

  * These tests are a port of the upstream suite, so they keep the upstream
    test class and method names rather than the CodeBrix
    <ClassUnderTest>Tests / snake_case convention. Follow the surrounding file
    when adding to an existing test class.


NOTES
================================================================================

  * The Netconf/ folder name does not match the CodeBrix.SSH.NetConf namespace
    it declares; this mirrors upstream and is intentional. Do not "fix" it
    without also updating THIRD-PARTY-NOTICES.txt's file-mapping claims.

  * Session is a public class implementing the internal ISession interface, and
    its constructor is internal. It is plumbing, not a consumer entry point.

  * ForwardedPortStatus is internal despite reading like a public enum-ish
    type; do not document it as consumer API.

  * SshNetLoggingConfiguration exposes WiresharkKeyLogFilePath only in DEBUG
    builds. It writes session secrets to disk -- never enable it in a shipped
    configuration.

  * The eight AI-agent pointer files (AGENTS.md, CLAUDE.md, .clinerules,
    .cursorrules, .cursor/rules/agent-readme.mdc, .windsurfrules,
    .github/copilot-instructions.md, .junie/guidelines.md) are maintained
    centrally across the CodeBrix family and all point at README-INDEX.txt. Do
    not edit them here.
