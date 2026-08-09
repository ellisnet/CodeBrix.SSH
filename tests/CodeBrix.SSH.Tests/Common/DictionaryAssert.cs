using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

public static class DictionaryAssert
{
    public static void AreEqual<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
    {
        if (ReferenceEquals(expected, actual))
        {
            return;
        }

        if (expected == null)
        {
            throw new XunitException("Expected dictionary to be null, but was not null.");
        }

        if (actual == null)
        {
            throw new XunitException("Expected dictionary not to be null, but was null.");
        }

        if (expected.Count != actual.Count)
        {
            throw new XunitException(string.Format("Expected dictionary to contain {0} entries, but was {1}.",
                                                          expected.Count, actual.Count));
        }

        foreach (var expectedEntry in expected)
        {
            if (!actual.TryGetValue(expectedEntry.Key, out var actualValue))
            {
                throw new XunitException(string.Format("Dictionary contains no entry with key '{0}'.", expectedEntry.Key));
            }

            if (!Equals(expectedEntry.Value, actualValue))
            {
                throw new XunitException(string.Format("Value for key '{0}' does not match.", expectedEntry.Key));
            }
        }
    }
}
