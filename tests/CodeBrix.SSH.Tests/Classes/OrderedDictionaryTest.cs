using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class OrderedDictionaryTest
{
    private static void AssertEqual<TKey, TValue>(List<KeyValuePair<TKey, TValue>> expected, OrderedDictionary<TKey, TValue> o)
    {
        Assert.Equal(expected.Count, o.Count);

        Assert.Equal(expected, ToList(o)); // Test the enumerator

        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], o.GetAt(i));

            Assert.Equal(expected[i].Value, o[expected[i].Key]);

            Assert.True(o.TryGetValue(expected[i].Key, out TValue value));
            Assert.Equal(expected[i].Value, value);

            Assert.True(o.TryGetValue(expected[i].Key, out value, out int index));
            Assert.Equal(expected[i].Value, value);
            Assert.Equal(i, index);

            Assert.True(((ICollection<KeyValuePair<TKey, TValue>>)o).Contains(expected[i]));
            Assert.True(o.ContainsKey(expected[i].Key));
            Assert.True(o.ContainsValue(expected[i].Value));
            Assert.True(o.Keys.Contains(expected[i].Key));
            Assert.True(o.Values.Contains(expected[i].Value));

            Assert.Equal(i, o.IndexOf(expected[i].Key));

            Assert.False(o.TryAdd(expected[i].Key, default));
            Assert.False(o.TryAdd(expected[i].Key, default, out index));
            Assert.Equal(i, index);
        }

        Assert.Equal(expected.Count, o.Keys.Count);
        Assert.Equal(expected.Select(kvp => kvp.Key).ToList(), ToList(o.Keys));
        Assert.Equal(ToList(o.Keys), ToList(((IReadOnlyDictionary<TKey, TValue>)o).Keys));

        Assert.Equal(expected.Count, o.Values.Count);
        Assert.Equal(expected.Select(kvp => kvp.Value).ToList(), ToList(o.Values));
        Assert.Equal(ToList(o.Values), ToList(((IReadOnlyDictionary<TKey, TValue>)o).Values));

        // Test CopyTo
        var kvpArray = new KeyValuePair<TKey, TValue>[1 + expected.Count + 1];
        ((ICollection<KeyValuePair<TKey, TValue>>)o).CopyTo(kvpArray, 1);
        Assert.Equal(
            (List<KeyValuePair<TKey, TValue>>)[default, .. expected, default],
            kvpArray);

        var keysArray = new TKey[1 + expected.Count + 1];
        o.Keys.CopyTo(keysArray, 1);
        Assert.Equal(
            (List<TKey>)[default, .. expected.Select(kvp => kvp.Key), default],
            keysArray);

        var valuesArray = new TValue[1 + expected.Count + 1];
        o.Values.CopyTo(valuesArray, 1);
        Assert.Equal(
            (List<TValue>)[default, .. expected.Select(kvp => kvp.Value), default],
            valuesArray);

        // Creates a List<T> via enumeration, avoiding the ICollection<T>.CopyTo
        // optimisation in the List<T> constructor.
        static List<T> ToList<T>(IEnumerable<T> values)
        {
            List<T> list = new();
            foreach (T t in values)
            {
                list.Add(t);
            }
            return list;
        }
    }

    [Fact]
    public void NullKey_ThrowsArgumentNull()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        Assert.Throws<ArgumentNullException>(() => o[null]);
        Assert.Throws<ArgumentNullException>(() => o.Add(null, 1));
        Assert.Throws<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Add(new KeyValuePair<string, int>(null, 1)));
        Assert.Throws<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>(null, 1)));
        Assert.Throws<ArgumentNullException>(() => o.ContainsKey(null));
        Assert.Throws<ArgumentNullException>(() => o.IndexOf(null));
        Assert.Throws<ArgumentNullException>(() => o.Insert(0, null, 1));
        Assert.Throws<ArgumentNullException>(() => o.Remove(null, out _));
        Assert.Throws<ArgumentNullException>(() => o.Remove(null));
        Assert.Throws<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>(null, 1)));
        Assert.Throws<ArgumentNullException>(() => o.SetAt(0, null, 1));
        Assert.Throws<ArgumentNullException>(() => o.SetPosition(null, 0));
        Assert.Throws<ArgumentNullException>(() => o.TryAdd(null, 1));
        Assert.Throws<ArgumentNullException>(() => o.TryAdd(null, 1, out _));
        Assert.Throws<ArgumentNullException>(() => o.TryGetValue(null, out _));
        Assert.Throws<ArgumentNullException>(() => o.TryGetValue(null, out _, out _));
    }

    [Fact]
    public void Indexer_Match_GetterReturnsValue()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

        Assert.Equal(8, o["b"]);
    }

    [Fact]
    public void Indexer_Match_SetterChangesValue()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

        o["a"] = 5;

        AssertEqual([new("a", 5), new("b", 8)], o);
    }

    [Fact]
    public void Indexer_NoMatch_GetterThrowsKeyNotFound()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        Assert.Throws<KeyNotFoundException>(() => o["b"]);
    }

    [Fact]
    public void Indexer_NoMatch_SetterAddsItem()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        o["b"] = 8;

        AssertEqual([new("a", 4), new("b", 8)], o);
    }

    [Fact]
    public void Add_Match()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        Assert.Throws<ArgumentException>(() => o.Add("a", 8));
        Assert.Throws<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).Add(new KeyValuePair<string, int>("a", 8)));
    }

    [Fact]
    public void Clear()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

        AssertEqual([new("a", 4), new("b", 8)], o);
        o.Clear();
        AssertEqual([], o);
    }

    [Fact]
    public void CopyTo()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

        Assert.Throws<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], -1));
        Assert.Throws<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], 3));
        Assert.Throws<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], 2));

        Assert.Throws<ArgumentNullException>(() => o.Keys.CopyTo(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Keys.CopyTo(new string[3], -1));
        Assert.Throws<ArgumentException>(() => o.Keys.CopyTo(new string[3], 3));
        Assert.Throws<ArgumentException>(() => o.Keys.CopyTo(new string[3], 2));

        Assert.Throws<ArgumentNullException>(() => o.Values.CopyTo(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Values.CopyTo(new int[3], -1));
        Assert.Throws<ArgumentException>(() => o.Values.CopyTo(new int[3], 3));
        Assert.Throws<ArgumentException>(() => o.Values.CopyTo(new int[3], 2));
    }

    [Fact]
    public void ContainsKvp_ChecksKeyAndValue()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        Assert.False(((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>("a", 8)));
        Assert.True(((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>("a", 4)));
    }

    [Fact]
    public void NullValues_Permitted()
    {
        OrderedDictionary<string, string> o = new() { { "a", "1" } };

        Assert.False(o.ContainsValue(null));

        o.Add("b", null);

        AssertEqual([new("a", "1"), new("b", null)], o);
    }

    [Fact]
    public void GetAt_OutOfRange()
    {
        OrderedDictionary<string, string> o = new() { { "a", "1" } };

        Assert.Throws<ArgumentOutOfRangeException>(() => o.GetAt(-2));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.GetAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.GetAt(1));
    }

    [Fact]
    public void RemoveKvp_ChecksKeyAndValue()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 } };

        Assert.False(((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>("a", 8)));
        AssertEqual([new("a", 4)], o);

        Assert.True(((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>("a", 4)));
        AssertEqual([], o);
    }

    [Fact]
    public void SetAt()
    {
        OrderedDictionary<string, double> o = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(-2, 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(-1, 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(0, 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(1, 1.1));

        o.Add("a", 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(-2, 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(-1, 1.1));

        o.SetAt(0, 1.1);

        AssertEqual([new("a", 1.1)], o);

        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(1, 5.5));
    }

    [Fact]
    public void SetAt3Params_OutOfRange()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(-1, "d", 16));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetAt(3, "d", 16));
    }

    [Fact]
    public void SetAt3Params_ExistingKeyCorrectIndex_PermitsChangingValue()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

        o.SetAt(2, "c", 16);

        AssertEqual([new("a", 4), new("b", 8), new("c", 16)], o);
    }

    [Fact]
    public void SetAt3Params_ExistingKeyDifferentIndex_Throws()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

        Assert.Throws<ArgumentException>(() => o.SetAt(1, "c", 16));
    }

    [Fact]
    public void SetAt3Params_PermitsChangingToNewKey()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

        o.SetAt(1, "d", 16);

        AssertEqual([new("a", 4), new("d", 16), new("c", 12)], o);
    }

    [Fact]
    public void Get_NonExistent()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 } };

        Assert.Throws<KeyNotFoundException>(() => o["doesn't exist"]);
        Assert.False(((ICollection<KeyValuePair<string, float>>)o).Contains(new KeyValuePair<string, float>("doesn't exist", 1)));
        Assert.False(o.ContainsKey("doesn't exist"));
        Assert.False(o.ContainsValue(999));
        Assert.Equal(-1, o.IndexOf("doesn't exist"));

        Assert.False(o.Remove("doesn't exist", out float value));
        Assert.Equal(default, value);

        Assert.False(o.Remove("doesn't exist"));

        Assert.False(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("doesn't exist", 1)));

        Assert.False(o.TryGetValue("doesn't exist", out value));
        Assert.Equal(default, value);

        Assert.False(o.TryGetValue("doesn't exist", out value, out int index));
        Assert.Equal(default, value);
        Assert.Equal(-1, index);

        AssertEqual([new("a", 4)], o);
    }

    [Fact]
    public void Insert()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 } };

        Assert.Throws<ArgumentOutOfRangeException>(() => o.Insert(-1, "c", 12));

        o.Insert(0, "c", 12); // Start
        AssertEqual([new("c", 12), new("a", 4), new("b", 8)], o);

        o.Insert(2, "d", 12); // Middle
        AssertEqual([new("c", 12), new("a", 4), new("d", 12), new("b", 8)], o);

        o.Insert(o.Count, "e", 16); // End
        AssertEqual([new("c", 12), new("a", 4), new("d", 12), new("b", 8), new("e", 16)], o);

        Assert.Throws<ArgumentOutOfRangeException>(() => o.Insert(o.Count + 1, "f", 16));

        // Existing key
        Assert.Throws<ArgumentException>(() => o.Insert(0, "a", 12));
    }

    [Fact]
    public void Remove_Success()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

        Assert.True(o.Remove("b"));
        AssertEqual([new("a", 4), new("c", 12)], o);

        Assert.True(o.Remove("a", out float value));
        Assert.Equal(4, value);
        AssertEqual([new("c", 12)], o);

        // ICollection.Remove must match Key and Value
        Assert.False(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("c", -1)));
        AssertEqual([new("c", 12)], o);

        Assert.True(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("c", 12)));
        AssertEqual([], o);
    }

    [Fact]
    public void RemoveAt()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

        Assert.Throws<ArgumentOutOfRangeException>(() => o.RemoveAt(-2));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.RemoveAt(-1));

        o.RemoveAt(0); // Start
        AssertEqual([new("b", 8), new("c", 12), new("d", 16)], o);

        o.RemoveAt(1); // Middle
        AssertEqual([new("b", 8), new("d", 16)], o);

        o.RemoveAt(1); // End
        AssertEqual([new("b", 8)], o);

        Assert.Throws<ArgumentOutOfRangeException>(() => o.RemoveAt(1));
    }

    [Fact]
    public void SetPosition_ByIndex()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

        ArgumentOutOfRangeException ex;

        ex = Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition(-1, 0));
        Assert.Equal("index", ex.ParamName);

        ex = Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition(0, -1));
        Assert.Equal("newIndex", ex.ParamName);

        ex = Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition(0, 4));
        Assert.Equal("newIndex", ex.ParamName);

        ex = Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition(4, 0));
        Assert.Equal("index", ex.ParamName);

        o.SetPosition(1, 0);
        AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

        o.SetPosition(0, 1);
        AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

        o.SetPosition(1, 2);
        AssertEqual([new("a", 4), new("c", 12), new("b", 8), new("d", 16)], o);

        o.SetPosition(2, 1);
        AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

        o.SetPosition(0, 3);
        AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);

        o.SetPosition(3, 1);
        AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

        o.SetPosition(1, 1); // No-op
        AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);
    }

    [Fact]
    public void SetPosition_ByKey()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition("a", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.SetPosition("a", 4));
        Assert.Throws<KeyNotFoundException>(() => o.SetPosition("e", 0));

        o.SetPosition("b", 0);
        AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

        o.SetPosition("b", 1);
        AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

        o.SetPosition("a", 3);
        AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);

        o.SetPosition("d", 2); // No-op
        AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);
    }

    [Fact]
    public void TryAdd_Success()
    {
        OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 } };

        Assert.True(o.TryAdd("c", 12));

        AssertEqual([new("a", 4), new("b", 8), new("c", 12)], o);

        Assert.True(o.TryAdd("d", 16, out int index));
        Assert.Equal(3, index);

        AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);
    }

    [Fact]
    public void KeysAndValuesAreReadOnly()
    {
        OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

        Assert.True(o.Keys.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => o.Keys.Add("c"));
        Assert.Throws<NotSupportedException>(o.Keys.Clear);
        Assert.Throws<NotSupportedException>(() => o.Keys.Remove("a"));

        Assert.True(o.Values.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => o.Values.Add(12));
        Assert.Throws<NotSupportedException>(o.Values.Clear);
        Assert.Throws<NotSupportedException>(() => o.Values.Remove(4));
    }
}
