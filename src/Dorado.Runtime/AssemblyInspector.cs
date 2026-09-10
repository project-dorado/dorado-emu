using Mono.Cecil;

namespace Dorado.Runtime;

/// <summary>A summary of the assembly and member references in a managed assembly.</summary>
public sealed class AssemblyReferenceReport
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> AssemblyReferences { get; init; }

    /// <summary>Referenced members grouped by declaring type (for example <c>Microsoft.Xna.Framework.Game::Run</c>).</summary>
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> MemberReferences { get; init; }

    /// <summary>Base types of the module's own types that derive from XNA types.</summary>
    public required IReadOnlyList<string> XnaDerivedTypes { get; init; }

    /// <summary>All referenced types, as <c>scope|Full.Name</c>, sorted.</summary>
    public required IReadOnlyList<string> TypeReferences { get; init; }
}

/// <summary>Reads metadata from Zune app assemblies without executing them.</summary>
public static class AssemblyInspector
{
    public static AssemblyReferenceReport Inspect(string path)
    {
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = false });

        var assemblies = module.AssemblyReferences
            .Select(r => r.FullName)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        var members = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (MemberReference reference in module.GetMemberReferences())
        {
            TypeReference declaringType = reference.DeclaringType;
            string type = declaringType?.FullName ?? "<unknown>";
            if (declaringType is not null)
            {
                string scope = declaringType.Scope switch
                {
                    AssemblyNameReference assembly => assembly.Name,
                    ModuleDefinition => "<self>",
                    _ => declaringType.Scope?.ToString() ?? "?",
                };
                type = $"{scope}|{type}";
            }

            if (!members.TryGetValue(type, out var set))
            {
                set = new SortedSet<string>(StringComparer.Ordinal);
                members[type] = set;
            }

            set.Add(reference.FullName);
        }

        var derived = new List<string>();
        foreach (TypeDefinition type in module.Types)
        {
            if (type.BaseType?.FullName is { } baseName && baseName.StartsWith("Microsoft.Xna", StringComparison.Ordinal))
            {
                derived.Add($"{type.FullName} : {baseName}");
            }
        }

        var typeRefs = new SortedSet<string>(StringComparer.Ordinal);
        foreach (TypeReference type in module.GetTypeReferences())
        {
            string scope = type.Scope switch
            {
                AssemblyNameReference assembly => assembly.Name,
                ModuleDefinition => "<self>",
                _ => type.Scope?.ToString() ?? "?",
            };
            typeRefs.Add($"{scope}|{type.FullName}");
        }

        return new AssemblyReferenceReport
        {
            Path = path,
            AssemblyReferences = assemblies,
            TypeReferences = typeRefs.ToArray(),
            MemberReferences = members.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyList<string>)kv.Value.ToArray(),
                StringComparer.Ordinal),
            XnaDerivedTypes = derived,
        };
    }
}
