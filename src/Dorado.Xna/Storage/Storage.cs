using System.Runtime.InteropServices;
using Dorado.Platform;

namespace Microsoft.Xna.Framework.Storage;

/// <summary>The local storage device Dorado exposes to a running title.</summary>
public sealed class StorageDevice
{
    private static readonly StorageDevice Instance = new();

    private StorageDevice()
    {
    }

    public static event EventHandler<EventArgs>? DeviceChanged;

    internal static StorageDevice Current => Instance;

    public long FreeSpace => TryDrive(info => info.AvailableFreeSpace);

    public bool IsConnected => true;

    public long TotalSpace => TryDrive(info => info.TotalSize);

    public StorageContainer OpenContainer(string titleName)
    {
        string name = string.IsNullOrWhiteSpace(titleName) ? "title" : titleName;
        string path = Path.Combine(TitleDirectory(), Sanitize(name));
        Directory.CreateDirectory(path);
        return new StorageContainer(this, path, name);
    }

    public void DeleteContainer(string titleName)
    {
        string name = string.IsNullOrWhiteSpace(titleName) ? "title" : titleName;
        string path = Path.Combine(TitleDirectory(), Sanitize(name));
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    internal static string TitleDirectory()
    {
        string title = string.IsNullOrWhiteSpace(PlatformHost.GameTitle) ? "Dorado" : PlatformHost.GameTitle;
        return Path.Combine(PlatformHost.StorageRoot, Sanitize(title));
    }

    internal static void RaiseDeviceChanged() => DeviceChanged?.Invoke(Instance, EventArgs.Empty);

    private static long TryDrive(Func<DriveInfo, long> read)
    {
        try
        {
            string root = Path.GetPathRoot(Path.GetFullPath(PlatformHost.StorageRoot)) ?? Path.DirectorySeparatorChar.ToString();
            var info = new DriveInfo(root);
            return read(info);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return 0;
        }
    }

    private static string Sanitize(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        string cleaned = new string(chars).Trim();
        return cleaned.Length == 0 ? "title" : cleaned;
    }
}

/// <summary>A title-scoped directory on the storage device.</summary>
public class StorageContainer : IDisposable
{
    private readonly StorageDevice _device;

    internal StorageContainer(StorageDevice device, string path, string titleName)
    {
        _device = device;
        Path = path;
        TitleName = titleName;
    }

    public bool IsDisposed { get; private set; }

    public string Path { get; }

    public StorageDevice StorageDevice => _device;

    public static string TitleLocation => PlatformHost.AppDirectory;

    public string TitleName { get; }

    public event EventHandler? Disposing;

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        Disposing?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }
}

/// <summary>Raised when the storage device is not available.</summary>
[Serializable]
public class StorageDeviceNotConnectedException : ExternalException
{
    public StorageDeviceNotConnectedException()
        : base("The storage device is not connected.")
    {
    }

    public StorageDeviceNotConnectedException(string message)
        : base(message)
    {
    }

    public StorageDeviceNotConnectedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
