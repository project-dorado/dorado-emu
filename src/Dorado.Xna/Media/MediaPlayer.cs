namespace Microsoft.Xna.Framework.Media;

/// <summary>Deterministic, offline implementation of the Zune media-player surface.</summary>
public static class MediaPlayer
{
    private static readonly MediaQueue PlaybackQueue = new();
    private static MediaState state = MediaState.Stopped;
    private static float volume = 1f;

    /// <summary>Raised when the active song changes.</summary>
    public static event EventHandler? ActiveSongChanged;

    /// <summary>Raised when the playback state changes.</summary>
    public static event EventHandler? MediaStateChanged;

    public static bool GameHasControl => true;

    public static bool IsMuted { get; set; }

    public static bool IsRepeating { get; set; }

    public static bool IsShuffled { get; set; }

    public static bool IsVisualizationEnabled { get; set; }

    public static TimeSpan PlayPosition => TimeSpan.Zero;

    public static MediaQueue Queue => PlaybackQueue;

    public static MediaState State => state;

    public static float Volume
    {
        get => volume;
        set => volume = Math.Clamp(value, 0f, 1f);
    }

    public static void Play(Song song)
    {
        ArgumentNullException.ThrowIfNull(song);
        PlaybackQueue.SetQueue([song], 0);
        SetState(MediaState.Playing);
    }

    public static void Play(SongCollection songs)
    {
        ArgumentNullException.ThrowIfNull(songs);
        Play(songs, 0);
    }

    public static void Play(SongCollection songs, int index)
    {
        ArgumentNullException.ThrowIfNull(songs);
        if (songs.Count == 0)
        {
            PlaybackQueue.SetQueue([], -1);
            SetState(MediaState.Stopped);
            return;
        }

        if (index < 0 || index >= songs.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        PlaybackQueue.SetQueue(songs, index);
        SetState(MediaState.Playing);
    }

    public static void Pause()
    {
        if (state == MediaState.Playing)
        {
            SetState(MediaState.Paused);
        }
    }

    public static void Resume()
    {
        if (state != MediaState.Playing)
        {
            SetState(MediaState.Playing);
        }
    }

    public static void Stop() => SetState(MediaState.Stopped);

    public static void MoveNext() => PlaybackQueue.MoveNext();

    public static void MovePrevious() => PlaybackQueue.MovePrevious();

    public static void GetVisualizationData(VisualizationData visualizationData)
    {
        ArgumentNullException.ThrowIfNull(visualizationData);
    }

    internal static void NotifyActiveSongChanged() => ActiveSongChanged?.Invoke(null, EventArgs.Empty);

    private static void SetState(MediaState newState)
    {
        if (state == newState)
        {
            return;
        }

        state = newState;
        MediaStateChanged?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary>The songs queued through <see cref="MediaPlayer"/>.</summary>
public sealed class MediaQueue
{
    private readonly List<Song> songs = [];
    private int activeSongIndex = -1;

    internal MediaQueue()
    {
    }

    public Song? ActiveSong =>
        activeSongIndex >= 0 && activeSongIndex < songs.Count ? songs[activeSongIndex] : null;

    public int ActiveSongIndex
    {
        get => activeSongIndex;
        set => MoveTo(value);
    }

    public int Count => songs.Count;

    public Song this[int index] => songs[index];

    internal void SetQueue(IEnumerable<Song> queue, int activeIndex)
    {
        songs.Clear();
        songs.AddRange(queue);

        int index = songs.Count == 0 ? -1 : Math.Clamp(activeIndex, 0, songs.Count - 1);
        bool changed = index != activeSongIndex;
        activeSongIndex = index;

        if (changed)
        {
            MediaPlayer.NotifyActiveSongChanged();
        }
    }

    internal void MoveNext()
    {
        if (songs.Count > 0)
        {
            MoveTo(activeSongIndex < songs.Count - 1 ? activeSongIndex + 1 : 0);
        }
    }

    internal void MovePrevious()
    {
        if (songs.Count > 0)
        {
            MoveTo(activeSongIndex > 0 ? activeSongIndex - 1 : songs.Count - 1);
        }
    }

    private void MoveTo(int index)
    {
        if (songs.Count == 0)
        {
            activeSongIndex = -1;
            return;
        }

        int clamped = Math.Clamp(index, 0, songs.Count - 1);
        if (clamped == activeSongIndex)
        {
            return;
        }

        activeSongIndex = clamped;
        MediaPlayer.NotifyActiveSongChanged();
    }
}

public enum MediaState
{
    Playing = 1,
    Paused = 2,
    Stopped = 0,
}
