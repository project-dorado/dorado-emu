using System.Collections;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>The base identity of a local or networked gamer.</summary>
public abstract class Gamer
{
    private protected Gamer(string gamertag)
    {
        Gamertag = gamertag;
    }

    public string Gamertag { get; }

    public object? Tag { get; set; }

    public bool IsDisposed { get; private set; }

    public static SignedInGamerCollection SignedInGamers { get; } = new();

    public override string ToString() => Gamertag;

    internal void MarkDisposed() => IsDisposed = true;
}

/// <summary>A gamer signed in on this device.</summary>
public sealed class SignedInGamer : Gamer
{
    internal SignedInGamer(string gamertag, PlayerIndex playerIndex)
        : base(gamertag)
    {
        PlayerIndex = playerIndex;
    }

    public PlayerIndex PlayerIndex { get; }

    public bool IsSignedInToLive => false;

    public bool IsGuest => false;

    public GameDefaults GameDefaults { get; } = new();

    public GamerPrivileges Privileges { get; } = new();

    public event EventHandler<SignedInEventArgs>? SignedIn;

    public event EventHandler<SignedOutEventArgs>? SignedOut;

    internal void RaiseSignedIn() => SignedIn?.Invoke(this, new SignedInEventArgs(this));

    internal void RaiseSignedOut() => SignedOut?.Invoke(this, new SignedOutEventArgs(this));
}

/// <summary>The signed-in gamers on this device, indexed by player.</summary>
public sealed class SignedInGamerCollection : GamerCollection<SignedInGamer>
{
    internal SignedInGamerCollection()
    {
    }

    public SignedInGamer? this[PlayerIndex index]
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].PlayerIndex == index)
                {
                    return this[i];
                }
            }

            return null;
        }
    }
}

/// <summary>A read-only, internally maintained collection of gamers.</summary>
public class GamerCollection<T> : ReadOnlyCollection<T>, IEnumerable<Gamer>
    where T : Gamer
{
    private readonly List<T> wrappedList;

    internal GamerCollection()
        : base(new List<T>())
    {
        wrappedList = (List<T>)Items;
    }

    public new GamerCollectionEnumerator GetEnumerator() => new(wrappedList.GetEnumerator());

    IEnumerator<Gamer> IEnumerable<Gamer>.GetEnumerator()
    {
        foreach (T gamer in wrappedList)
        {
            yield return gamer;
        }
    }

    /// <summary>An allocation-free enumerator over the collection.</summary>
    public struct GamerCollectionEnumerator : IEnumerator<T>, IEnumerator
    {
        private List<T>.Enumerator inner;

        internal GamerCollectionEnumerator(List<T>.Enumerator inner) => this.inner = inner;

        public readonly T Current => inner.Current;

        readonly object? IEnumerator.Current => inner.Current;

        public bool MoveNext() => inner.MoveNext();

        public void Dispose() => inner.Dispose();

        void IEnumerator.Reset() => ((IEnumerator)inner).Reset();
    }
}
