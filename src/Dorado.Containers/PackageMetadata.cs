namespace Dorado.Containers;

/// <summary>Descriptive fields lifted from a package manifest.</summary>
public sealed class PackageMetadata
{
    /// <summary>Display title (from <c>TITL</c> or <c>GameTitle</c>).</summary>
    public string? Title { get; init; }

    /// <summary>Long description, if present.</summary>
    public string? Description { get; init; }

    /// <summary>Copyright notice, if present.</summary>
    public string? Copyright { get; init; }

    /// <summary>Executable file name from the <c>EXEC</c> record (for example <c>Calculator.exe</c>).</summary>
    public string? Executable { get; init; }

    /// <summary>Managed assembly to launch (from <c>StartupAssembly</c>).</summary>
    public string? StartupAssembly { get; init; }

    /// <summary>Target platform (for example <c>Zune</c> or <c>Zune.v3.1</c>).</summary>
    public string? Platform { get; init; }

    /// <summary>XNA runtime profile name, when supplied by the package.</summary>
    public string? RuntimeProfile { get; init; }

    /// <summary>Package identity GUID (lower-case hex, no braces).</summary>
    public string? GameGuid { get; init; }

    /// <summary>The <c>CcgameVersion</c> value, when present.</summary>
    public string? CcgameVersion { get; init; }
}
