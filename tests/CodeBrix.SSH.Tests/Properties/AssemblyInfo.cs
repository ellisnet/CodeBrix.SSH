using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;   // ParallelMode
using Xunit.v3;    // ParallelizationAttribute

[assembly: ExcludeFromCodeCoverage] //was previously: test/Renci.SshNet/Properties/AssemblyInfo.cs

// Many of these tests bind listeners to a fixed local port, and several drive a
// stub SSH server on a well-known port. MSTest runs test classes sequentially, so
// upstream never had to think about it; xUnit parallelises test collections by
// default, which makes those tests collide with "Address already in use".
// Running sequentially restores the upstream execution model.
[assembly: Parallelization(Mode = ParallelMode.None)]
