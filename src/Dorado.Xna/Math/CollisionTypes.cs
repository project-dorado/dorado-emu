namespace Microsoft.Xna.Framework;

/// <summary>Describes how a volume relates to another volume.</summary>
public enum ContainmentType
{
    Disjoint = 0,
    Contains = 1,
    Intersects = 2,
}

/// <summary>Describes where a volume lies relative to a plane.</summary>
public enum PlaneIntersectionType
{
    Front = 0,
    Back = 1,
    Intersecting = 2,
}
