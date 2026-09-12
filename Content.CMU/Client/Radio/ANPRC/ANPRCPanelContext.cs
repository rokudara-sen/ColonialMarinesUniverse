using System.Linq;
using Content.Shared.CMU14.Radio;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     Everything the panel's pages need to read the set, derived once per state update instead of
///     re-derived in each page. Keeps the pages about presentation and leaves "is this set actually
///     usable" in one place, so a page can never disagree with the readout above it about whether
///     the radio is on.
/// </summary>
public sealed class ANPRCPanelContext(IPrototypeManager prototypes)
{
    private readonly IPrototypeManager _prototypes = prototypes;

    public ANPRCRadioState State { get; private set; } = default!;

    /// <summary>Worn, or staked down as a retrans station. Either way the set is deployed.</summary>
    public bool Deployed { get; private set; }

    /// <summary>
    ///     The set is switched on and its panel is alive. A 117G sitting on a bench programmes
    ///     exactly like one on a man's back, so this is what every control that only touches the
    ///     set's own memory is gated on: the memories, the waveform, squelch, volume, the station
    ///     callsign, the COMSEC keys. The server gates none of those on being worn either.
    /// </summary>
    public bool Powered { get; private set; }

    /// <summary>
    ///     Switched on and deployed, so the set can actually put something on the air. Only the
    ///     controls the server itself refuses off a stowed pack are gated on this - the search
    ///     receiver, the log printer, keying up.
    /// </summary>
    public bool Online { get; private set; }

    /// <summary>The active slot carries something to work: a net, or a raw frequency.</summary>
    public bool HasActiveNet { get; private set; }

    /// <summary>Online and sitting on a net. The set can key up.</summary>
    public bool Ready { get; private set; }

    public RadioChannelPrototype? ActiveChannel { get; private set; }

    public bool ActiveIsDirect { get; private set; }

    public RadioFrequency ActiveFrequency { get; private set; }

    public void SetState(ANPRCRadioState state)
    {
        State = state;

        ActiveIsDirect = state.FrequencyOverrides.TryGetValue(state.ActiveSlot, out var direct);
        ActiveChannel = null;

        if (ActiveIsDirect)
        {
            ActiveFrequency = direct;
        }
        else if (state.Presets.TryGetValue(state.ActiveSlot, out var channel) &&
                 _prototypes.TryIndex(channel, out var proto))
        {
            ActiveChannel = proto;
            ActiveFrequency = PlanFrequency(proto);
        }
        else
        {
            ActiveFrequency = RadioFrequency.Off;
        }

        Deployed = state.IsEquipped || state.Planted;
        Powered = state.Enabled;
        Online = Powered && Deployed;
        HasActiveNet = (ActiveIsDirect || ActiveChannel != null) && state.ActiveSlot >= 0;
        Ready = Online && HasActiveNet && !state.SweepEnabled;
    }

    /// <summary>
    ///     Frequencies come off the round's signal plan carried in the state. The prototype number
    ///     is only the book value and will be wrong on any round where the plan was re-rolled.
    /// </summary>
    public RadioFrequency PlanFrequency(RadioChannelPrototype proto)
    {
        return State.ChannelFrequencies.TryGetValue(proto.ID, out var frequency)
            ? frequency
            : proto.Frequency;
    }

    public IEnumerable<RadioChannelPrototype> Channels()
        => _prototypes.EnumeratePrototypes<RadioChannelPrototype>();

    public bool TryIndex(ProtoId<RadioChannelPrototype> id, out RadioChannelPrototype? proto)
        => _prototypes.TryIndex(id, out proto);

    public static string FormatFrequency(RadioFrequency frequency) => frequency.FormatMegahertz();

    public static string ModeShort(RadioMode mode) => mode switch
    {
        RadioMode.FrequencyHopping => "FH",
        RadioMode.SingleChannel => "SC",
        RadioMode.CipherText => "CT",
        RadioMode.PlainText => "PT",
        _ => "FH",
    };

    public static string ModeName(RadioMode mode) => mode switch
    {
        RadioMode.FrequencyHopping => "FREQ HOP",
        RadioMode.SingleChannel => "SINGLE CH",
        RadioMode.CipherText => "CIPHER",
        RadioMode.PlainText => "PLAIN",
        _ => "FREQ HOP",
    };

    public static string BandName(RadioFrequency frequency)
    {
        var kilohertz = frequency.Kilohertz;

        return kilohertz < 3_000 ? "LF"
            : kilohertz < 30_000 ? "HF"
            : kilohertz < 88_000 ? "VHF"
            : kilohertz < 300_000 ? "UHF"
            : "SHF";
    }

    /// <summary>
    ///     A bar graph in characters. Drawn out of plain ASCII rather than block-drawing glyphs:
    ///     the panel is lettered in RobotoMono throughout, which carries no block elements, so
    ///     the bars came out of the font as missing-glyph boxes.
    /// </summary>
    public static string Bars(int filled, int total)
    {
        filled = Math.Clamp(filled, 0, total);

        return new string('|', filled) + new string('.', total - filled);
    }

    /// <summary>
    ///     The digits of a partial fix, with everything the operator has not earned yet masked out.
    ///     259.237 walks up through 2XX.XXX, 25X.XXX, 259.XXX and then the whole number.
    /// </summary>
    public static string MaskDigits(string formatted, int tier, int tierMax)
    {
        if (tier >= tierMax)
            return formatted;

        var earned = Math.Max(0, tier);
        var seen = 0;
        var masked = formatted.ToCharArray();

        for (var i = 0; i < masked.Length; i++)
        {
            if (!char.IsAsciiDigit(masked[i]))
                continue;

            seen++;

            if (seen > earned)
                masked[i] = 'X';
        }

        return new string(masked);
    }

    public string SlotLabel(int slot)
        => State.SlotLabels.TryGetValue(slot, out var label) ? label : $"P{slot + 1}";

    /// <summary>What is loaded in a slot, rendered the way the panel prints it everywhere.</summary>
    public string SlotContents(int slot)
    {
        if (State.FrequencyOverrides.TryGetValue(slot, out var direct))
        {
            // a number the search receiver fixed but could not name reads as an unknown net,
            // so an operator can tell a tuned intercept from a number they punched in
            var unknown = State.SweepContacts.Any(contact =>
                contact.Resolved && !contact.Known && contact.Frequency == direct);

            return unknown
                ? $"{FormatFrequency(direct)} MHz  UNKNOWN NET"
                : $"{FormatFrequency(direct)} MHz  DIRECT";
        }

        if (State.Presets.TryGetValue(slot, out var channel) && _prototypes.TryIndex(channel, out var proto))
            return $"{FormatFrequency(PlanFrequency(proto))} MHz  {proto.LocalizedName.ToUpperInvariant()}";

        return "--- EMPTY ---";
    }
}
