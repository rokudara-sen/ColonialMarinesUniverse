namespace Content.Shared._AU14.Radio;

/// <summary>
/// The physical corded handset of an ANPRC manpack. Lives in a container slot
/// on the pack; taking it puts it in a hand, hanging up — or breaking the
/// cord — snaps it back onto the pack.
/// </summary>
[RegisterComponent]
public sealed partial class ANPRCHandsetComponent : Component
{
    /// <summary>Server-side: the pack this handset is wired into.</summary>
    [DataField]
    public EntityUid? Radio;
}
