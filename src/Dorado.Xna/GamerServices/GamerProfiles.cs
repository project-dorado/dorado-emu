using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>How racing cameras are positioned for a profile.</summary>
public enum RacingCameraAngle
{
    Back = 0,
    Front = 1,
    Inside = 2,
}

/// <summary>Preferred controller sensitivity for a profile.</summary>
public enum ControllerSensitivity
{
    Low = 0,
    Medium = 1,
    High = 2,
}

/// <summary>Preferred difficulty for a profile.</summary>
public enum GameDifficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
}

/// <summary>The gamer zone recorded for a profile.</summary>
public enum GamerZone
{
    Unknown = 0,
    Recreation = 1,
    Pro = 2,
    Family = 3,
    Underground = 4,
}

/// <summary>Whether a privilege is blocked, friends-only or open.</summary>
public enum GamerPrivilegeSetting
{
    Blocked = 0,
    FriendsOnly = 1,
    Everyone = 2,
}

/// <summary>
/// Per-profile game preferences. Dorado runs a single local, offline profile,
/// so every preference reports the XNA default.
/// </summary>
public sealed class GameDefaults
{
    public bool AccelerateWithButtons => false;

    public bool AutoAim => true;

    public bool AutoCenter => true;

    public bool BrakeWithButtons => false;

    public ControllerSensitivity ControllerSensitivity => ControllerSensitivity.Medium;

    public GameDifficulty GameDifficulty => GameDifficulty.Normal;

    public bool InvertYAxis => false;

    public bool ManualTransmission => false;

    public bool MoveWithRightThumbStick => false;

    public Color? PrimaryColor => null;

    public RacingCameraAngle RacingCameraAngle => RacingCameraAngle.Back;

    public Color? SecondaryColor => null;
}

/// <summary>The privileges of the local offline profile.</summary>
public sealed class GamerPrivileges
{
    public GamerPrivilegeSetting AllowCommunication => GamerPrivilegeSetting.Everyone;

    public bool AllowOnlineSessions => false;

    public GamerPrivilegeSetting AllowProfileViewing => GamerPrivilegeSetting.Everyone;

    public bool AllowPurchaseContent => false;

    public bool AllowTradeContent => false;

    public GamerPrivilegeSetting AllowUserCreatedContent => GamerPrivilegeSetting.Everyone;
}

/// <summary>Carries the gamer that signed in.</summary>
public class SignedInEventArgs : EventArgs
{
    public SignedInEventArgs(SignedInGamer gamer) =>
        Gamer = gamer ?? throw new ArgumentNullException(nameof(gamer));

    public SignedInGamer Gamer { get; }
}

/// <summary>Carries the gamer that signed out.</summary>
public class SignedOutEventArgs : EventArgs
{
    public SignedOutEventArgs(SignedInGamer gamer) =>
        Gamer = gamer ?? throw new ArgumentNullException(nameof(gamer));

    public SignedInGamer Gamer { get; }
}
