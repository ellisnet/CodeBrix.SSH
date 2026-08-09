using System.Globalization;
using System.Resources;

namespace CodeBrix.SSH.Tests.Properties; //was previously: Renci.SshNet.Tests.Properties;

/// <summary>
/// A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
/// <remarks>
/// Upstream ships a StronglyTypedResourceBuilder-generated version of this class.
/// It is hand-written here so that it conforms to the CodeBrix file layout rules,
/// and exposes only the members the test suite actually uses.
/// </remarks>
internal static class Resources
{
    private static readonly ResourceManager ResourceManagerInstance =
        new ResourceManager("CodeBrix.SSH.Tests.Properties.Resources", typeof(Resources).Assembly);

    private static CultureInfo resourceCulture;

    /// <summary>
    /// Gets the cached <see cref="System.Resources.ResourceManager"/> instance used by this class.
    /// </summary>
    internal static ResourceManager ResourceManager
    {
        get { return ResourceManagerInstance; }
    }

    /// <summary>
    /// Gets or sets the culture used for resource lookups by this strongly-typed resource class.
    /// </summary>
    internal static CultureInfo Culture
    {
        get { return resourceCulture; }
        set { resourceCulture = value; }
    }

    /// <summary>
    /// Gets the localized string for the "HOST" resource.
    /// </summary>
    internal static string HOST
    {
        get { return ResourceManagerInstance.GetString("HOST", resourceCulture); }
    }

    /// <summary>
    /// Gets the localized string for the "PASSWORD" resource.
    /// </summary>
    internal static string PASSWORD
    {
        get { return ResourceManagerInstance.GetString("PASSWORD", resourceCulture); }
    }

    /// <summary>
    /// Gets the localized string for the "PORT" resource.
    /// </summary>
    internal static string PORT
    {
        get { return ResourceManagerInstance.GetString("PORT", resourceCulture); }
    }

    /// <summary>
    /// Gets the localized string for the "PROXY_HOST" resource.
    /// </summary>
    internal static string PROXY_HOST
    {
        get { return ResourceManagerInstance.GetString("PROXY_HOST", resourceCulture); }
    }

    /// <summary>
    /// Gets the localized string for the "PROXY_PORT" resource.
    /// </summary>
    internal static string PROXY_PORT
    {
        get { return ResourceManagerInstance.GetString("PROXY_PORT", resourceCulture); }
    }

    /// <summary>
    /// Gets the localized string for the "USERNAME" resource.
    /// </summary>
    internal static string USERNAME
    {
        get { return ResourceManagerInstance.GetString("USERNAME", resourceCulture); }
    }
}
