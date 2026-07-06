namespace Content.Shared._AU14.Callsigns;

/// <summary>
/// Added to a mob via its job's roundComponents to claim a fixed callsign
/// suffix (6 = leader, 5 = 2IC, 7 = senior NCO, ROMEO = RTO, OPS = staff)
/// instead of an auto-assigned 1-N number.
/// </summary>
[RegisterComponent]
public sealed partial class AU14CallsignRoleComponent : Component
{
    [DataField(required: true)]
    public string Suffix = string.Empty;

    /// <summary>
    /// Always file this mob under the command element, even when squadded.
    /// When false, the mob uses its squad's element if it has one.
    /// </summary>
    [DataField]
    public bool CommandElement;
}
