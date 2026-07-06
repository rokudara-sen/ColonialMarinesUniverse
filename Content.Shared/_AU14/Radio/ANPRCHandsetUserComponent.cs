namespace Content.Shared._AU14.Radio;

/// <summary>
/// A mob holding the corded handset of a nearby ANPRC manpack. While it stays
/// in reach of the pack, the holder's :r speech transmits on the pack's active
/// net — no radio training required. This is how a squad leader talks to
/// command through their RTO.
/// </summary>
[RegisterComponent]
public sealed partial class ANPRCHandsetUserComponent : Component
{
    [DataField]
    public EntityUid Radio;

    /// <summary>Set between ChatGetPrefix and EntitySpoke for an :r transmission.</summary>
    public bool PendingTransmit;
}
