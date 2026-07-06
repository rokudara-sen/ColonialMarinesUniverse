using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._AU14.Callsigns;

/// <summary>
/// A faction member's assigned radio callsign, e.g. "ALPHA 6" or "HAVOC ROMEO".
/// Assigned automatically at spawn from job and squad; editable from the
/// callsign directory console. Replaces the speaker's name on all faction
/// radio traffic while transmitting.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AU14CallsignComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Faction = string.Empty;

    /// <summary>Full callsign as heard on the net: element word + suffix.</summary>
    [DataField, AutoNetworkedField]
    public string Callsign = string.Empty;

    /// <summary>The part after the element word: "6", "5", "ROMEO", "1-3"…</summary>
    [DataField, AutoNetworkedField]
    public string Suffix = string.Empty;

    [DataField, AutoNetworkedField]
    public string JobTitle = string.Empty;

    /// <summary>Squad entity this callsign is filed under; null = command element.</summary>
    [DataField]
    public EntityUid? Squad;

    /// <summary>
    /// True when the suffix came from a role (6/5/ROMEO…) or a manual console
    /// edit, so squad moves renumber only auto-assigned members.
    /// </summary>
    [DataField]
    public bool RoleSuffix;

    /// <summary>
    /// Server transient: tick during which this mob is putting a radio message
    /// on the air, so the speaker-name transform knows to mask it.
    /// </summary>
    public GameTick RadioMaskTick;
}
