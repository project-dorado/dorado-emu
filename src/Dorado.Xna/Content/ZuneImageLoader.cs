using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// Decodes image assets (PNG/JPEG/BMP) through Dorado's native ZDK image
/// bridge. Titles built for Windows hand <c>ContentManager</c> asset names
/// whose on-disk casing or extension differs; this keeps them loadable.
/// </summary>
internal static class ZuneImageLoader
{
    static ZuneImageLoader()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(ZuneImageLoader).Assembly, Resolve);
        }
        catch (InvalidOperationException)
        {
            // A resolver is already installed for the shim assembly.
        }
    }

    public static Texture2D Load(string path)
    {
        if (ZDKImage_CreateImageFromFile(path, out IntPtr handle) != 0)
        {
            throw new ContentLoadException($"Could not decode image '{path}'.");
        }

        try
        {
            if (ZDKImage_GetImageSize(handle, out uint width, out uint height) != 0)
            {
                throw new ContentLoadException($"Could not read image size for '{path}'.");
            }

            var pixels = new byte[width * height * 4];
            if (ZDKImage_GetImageData(handle, pixels, (uint)pixels.Length) != 0)
            {
                throw new ContentLoadException($"Could not read image data for '{path}'.");
            }

            return Texture2D.FromPixels((int)width, (int)height, pixels, premultiplied: false);
        }
        finally
        {
            _ = ZDKImage_ReleaseImage(handle);
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!libraryName.Equals("ZDK", StringComparison.OrdinalIgnoreCase) &&
            !libraryName.Equals("MEDIA", StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        string? configured = Environment.GetEnvironmentVariable("DORADO_ZDK_LIB");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured) &&
            NativeLibrary.TryLoad(configured, out IntPtr configuredHandle))
        {
            return configuredHandle;
        }

        foreach (string directory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            foreach (string name in new[] { "libZDK.so", "libZDK.dylib", "ZDK.dll" })
            {
                string candidate = Path.Combine(directory, name);
                if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out IntPtr handle))
                {
                    return handle;
                }
            }
        }

        return IntPtr.Zero;
    }

    [DllImport("ZDK")]
    private static extern uint ZDKImage_CreateImageFromFile(
        [MarshalAs(UnmanagedType.LPWStr)] string filename,
        out IntPtr hImage);

    [DllImport("ZDK")]
    private static extern uint ZDKImage_GetImageSize(IntPtr hImage, out uint cxSize, out uint cySize);

    [DllImport("ZDK")]
    private static extern uint ZDKImage_GetImageData(IntPtr hImage, byte[] buf, uint cbBuf);

    [DllImport("ZDK")]
    private static extern uint ZDKImage_ReleaseImage(IntPtr hImage);
}
