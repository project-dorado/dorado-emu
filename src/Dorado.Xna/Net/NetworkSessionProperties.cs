using System.Collections;

namespace Microsoft.Xna.Framework.Net;

/// <summary>Eight optional integer slots advertised with a network session.</summary>
public class NetworkSessionProperties : IList<int?>, ICollection<int?>, IEnumerable<int?>, IEnumerable
{
    private const int PropertyCount = 8;

    private readonly int?[] values = new int?[PropertyCount];

    public int? this[int index]
    {
        get
        {
            if (index < 0 || index >= PropertyCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return values[index];
        }

        set
        {
            if (index < 0 || index >= PropertyCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            values[index] = value;
        }
    }

    public int Count => PropertyCount;

    bool ICollection<int?>.IsReadOnly => false;

    public IEnumerator<int?> GetEnumerator() => ((IEnumerable<int?>)values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();

    int IList<int?>.IndexOf(int? item) => Array.IndexOf(values, item);

    bool ICollection<int?>.Contains(int? item) => Array.IndexOf(values, item) >= 0;

    void ICollection<int?>.CopyTo(int?[] array, int arrayIndex) => values.CopyTo(array, arrayIndex);

    void ICollection<int?>.Add(int? item) => throw new NotSupportedException();

    void IList<int?>.Insert(int index, int? item) => throw new NotSupportedException();

    bool ICollection<int?>.Remove(int? item) => throw new NotSupportedException();

    void IList<int?>.RemoveAt(int index) => throw new NotSupportedException();

    void ICollection<int?>.Clear() => throw new NotSupportedException();
}
