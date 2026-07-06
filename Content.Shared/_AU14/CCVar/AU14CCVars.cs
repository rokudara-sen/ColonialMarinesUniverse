using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._AU14.CCVar;

[CVarDefs]
public sealed partial class AU14CCVars : CVars
{
    /// <summary>
    /// TODO: Whether the AU14 entity fire spreading system is enabled.
    /// </summary>
    public static readonly CVarDef<bool> FireSpreading =
        CVarDef.Create("au14.fire_spreading", false, CVar.SERVERONLY);

    public static readonly CVarDef<bool> SellCargoRewards =
        CVarDef.Create("au14.sell_cargo_rewards", true, CVar.SERVERONLY);

    /// <summary>
    /// Master switch for the AU14 comms overhaul. When off, radio reverts to
    /// stock behavior: no ANPRC relay range gating, no jam/COMSEC garbling or
    /// squelch, no enemy-net interception, no DF blips, and no callsign
    /// assignment or name masking. ANPRC packs keep working as plain radios.
    /// Can be flipped mid-round; enabling it assigns callsigns to everyone
    /// who spawned while it was off.
    /// </summary>
    public static readonly CVarDef<bool> NewCommsSystem =
        CVarDef.Create("au14.new_comms_system", true, CVar.SERVERONLY);
}
