================================================================================
EXTRAS-README: CodeBrix.SSH
Samples, tools and other content in this repository that is not part of a
NuGet package
================================================================================

This repository contains NO samples, demo applications, benchmark projects or
build/utility tools. It builds exactly one library project and one test
project, and nothing else.


UNIT TEST PROJECT
================================================================================

    tests/CodeBrix.SSH.Tests/

The only non-package content in the repository. It is a port of the upstream
SSH.NET unit test suite to xUnit.v3, and it is entirely self-contained: it
runs offline, needs no SSH server, needs no Docker and opens no outbound
connections. Sockets it does bind are local listeners created and torn down by
the tests themselves.

Run it with:

    dotnet test CodeBrix.SSH.slnx

or, for a reliable exit code, by executing the built test assembly directly
with the native xUnit v3 runner:

    tests/CodeBrix.SSH.Tests/bin/Debug/net10.0/CodeBrix.SSH.Tests

See MAINTAINER-README.txt for the parallelisation rule, the mocking library in
use and a known flake in the VSTest bridge.

Beyond verifying the library, the suite doubles as a worked-example corpus for
consumers: AGENT-README.txt's "WORKING EXAMPLES ON GITHUB" section maps each
feature area to the test files that exercise it.


EMBEDDED TEST DATA
================================================================================

    tests/CodeBrix.SSH.Tests/Data/

Private keys, public keys, OpenSSH certificates and related sample data used
by the test suite, copied verbatim from the upstream SSH.NET repository and
compiled in as <EmbeddedResource> items. They cover every supported key type
(RSA, ECDSA 256/384/521, ED25519) across every supported container format
(OpenSSL traditional PEM, PKCS#8, ssh.com, OpenSSH, PuTTY v2/v3), encrypted
and unencrypted.

These are TEST FIXTURES, not credentials: they are published keys from a
public repository, they protect nothing, and they must never be used to
authenticate against a real host. They are not optional -- the key-parsing
tests fail without them -- and they are not shipped in the NuGet package.


REPOSITORY DOCUMENTATION FILES
================================================================================

    AGENT-README.txt        Consumer documentation for the NuGet package.
                            Also shipped inside the package.
    MAINTAINER-README.txt   Build, test, packaging and provenance notes.
    EXTRAS-README.txt       This file.
    README-INDEX.txt        Map of the README files in this repository.
    README.md               Human-facing overview, shown on GitHub and
                            nuget.org.
    THIRD-PARTY-NOTICES.txt Upstream licences reproduced in full, plus the
                            complete log of modifications made in this fork.
                            Also shipped inside the package.
    LICENSE                 MIT.
