namespace Content.Shared._AU14.Callsigns;

/// <summary>
/// Marks an entity as a callsign directory console for one faction's comms
/// net. Anyone with physical access can read it; editing element words and
/// member suffixes requires radio training (<see cref="Radio.ANPRCRadioUserComponent"/>)
/// and matching faction.
/// </summary>
[RegisterComponent]
public sealed partial class AU14CallsignConsoleComponent : Component
{
    [DataField(required: true)]
    public string Faction = string.Empty;
}
