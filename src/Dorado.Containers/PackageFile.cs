namespace Dorado.Containers;

/// <summary>A single file stored in a package.</summary>
/// <param name="Path">Logical path inside the package (for example <c>Content/circle.xnb</c>).</param>
/// <param name="Size">Length in bytes.</param>
/// <param name="ContainerName">The raw entry name as stored (for example <c>1</c> in a <c>.ccgame</c>).</param>
public sealed record PackageFile(string Path, long Size, string ContainerName);
