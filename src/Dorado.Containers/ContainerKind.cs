namespace Dorado.Containers;

/// <summary>The kind of Zune application container Dorado knows how to read.</summary>
public enum ContainerKind
{
    /// <summary>An XNA deployment cabinet (<c>.ccgame</c>), unencrypted.</summary>
    Ccgame,

    /// <summary>A marketplace NX container (<c>.zcp</c>), payload usually DRM-encrypted.</summary>
    Zcp,

    /// <summary>An extracted application tree (for example a device <c>\gametitle</c> dump).</summary>
    Directory,
}
