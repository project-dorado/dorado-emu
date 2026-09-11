using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>The gamer services surface Zune titles use: storage selection and trial state.</summary>
public static class Guide
{
    public static bool IsTrialMode => false;

    public static bool IsVisible => false;

    public static bool SimulateTrialMode { get; set; }

    public static IAsyncResult BeginShowStorageDeviceSelector(AsyncCallback? callback, object? state) =>
        Begin(callback, state);

    public static IAsyncResult BeginShowStorageDeviceSelector(
        int sizeInBytes,
        int directoryCount,
        AsyncCallback? callback,
        object? state) =>
        Begin(callback, state);

    public static IAsyncResult BeginShowStorageDeviceSelector(
        PlayerIndex player,
        AsyncCallback? callback,
        object? state) =>
        Begin(callback, state);

    public static IAsyncResult BeginShowStorageDeviceSelector(
        PlayerIndex player,
        int sizeInBytes,
        int directoryCount,
        AsyncCallback? callback,
        object? state) =>
        Begin(callback, state);

    public static StorageDevice EndShowStorageDeviceSelector(IAsyncResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result is not StorageDeviceAsyncResult selector)
        {
            throw new ArgumentException("The async result was not produced by Guide.", nameof(result));
        }

        return selector.Device;
    }

    public static void Show()
    {
    }

    private static IAsyncResult Begin(AsyncCallback? callback, object? state)
    {
        StorageDevice.RaiseDeviceChanged();
        var result = new StorageDeviceAsyncResult(state);
        callback?.Invoke(result);
        return result;
    }

    /// <summary>An already-completed result carrying the single local storage device.</summary>
    private sealed class StorageDeviceAsyncResult : IAsyncResult
    {
        public StorageDeviceAsyncResult(object? state)
        {
            AsyncState = state;
            AsyncWaitHandle = new ManualResetEvent(initialState: true);
        }

        public object? AsyncState { get; }

        public WaitHandle AsyncWaitHandle { get; }

        public bool CompletedSynchronously => true;

        public bool IsCompleted => true;

        public StorageDevice Device => StorageDevice.Current;
    }
}
