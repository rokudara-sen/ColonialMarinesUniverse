using Content.Shared.CMU14.Radio;
using Content.Shared.Radio;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     OPT: how the set is being worked rather than what it is tuned to. Every line here is one
///     radio setting, and ENT on a line steps it - which is how a set with no arrow keys is
///     configured.
/// </summary>
public sealed class ANPRCOptionsScreen : ANPRCScreen
{
    public override string Title => "OPT  SET CONFIG";

    public override string Status(ANPRCPanelContext context)
        => context.Deployed ? "DEPLOYED" : "STOWED";

    public static RadioMode NextMode(RadioMode mode) => mode switch
    {
        RadioMode.FrequencyHopping => RadioMode.SingleChannel,
        RadioMode.SingleChannel => RadioMode.CipherText,
        RadioMode.CipherText => RadioMode.PlainText,
        _ => RadioMode.FrequencyHopping,
    };

    public static RadioTxPower NextPower(RadioTxPower power) => power switch
    {
        RadioTxPower.Low => RadioTxPower.Medium,
        RadioTxPower.Medium => RadioTxPower.High,
        _ => RadioTxPower.Low,
    };

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;

        rows.Add(new ANPRCScreenRow
        {
            Label = "MODE",
            Value = ANPRCPanelContext.ModeName(state.Mode),
            Activate = () =>
            {
                var mode = NextMode(state.Mode);
                Radio.SetMode(mode);
                Host.Acknowledge("MODE " + ANPRCPanelContext.ModeName(mode));
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "MONITOR",
            Value = state.MonitorEnabled ? "ON" : "OFF",
            Lit = state.MonitorEnabled,
            Activate = () =>
            {
                Radio.ToggleMonitor();
                Host.Acknowledge(state.MonitorEnabled ? "MONITOR OFF" : "MONITOR ON");
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "SCAN",
            Value = state.ScanEnabled ? "ON" : "OFF",
            Lit = state.ScanEnabled,
            Activate = () =>
            {
                Radio.SetScan(!state.ScanEnabled);
                Host.Acknowledge(state.ScanEnabled ? "SCAN OFF" : "SCAN ON");
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "SQUELCH",
            Value = state.SquelchLevel.ToString(),
            Activate = () =>
            {
                var level = state.SquelchLevel >= ANPRCRadioComponent.MaxSquelchLevel
                    ? 0
                    : state.SquelchLevel + 1;

                Radio.SetSquelch(level);
                Host.Acknowledge("SQUELCH " + level);
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "OUTPUT",
            Value = state.TxPower.Short(),
            Activate = () =>
            {
                var power = NextPower(state.TxPower);
                Radio.SetTxPower(power);
                Host.Acknowledge("OUTPUT " + power.Short());
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "SPEAKER",
            Value = ANPRCVolume.Label(state.Volume),
            Lit = state.Volume > ANPRCRadioComponent.DefaultVolume,
            Style = state.Volume > ANPRCRadioComponent.DefaultVolume ? ANPRCRowStyle.Warn : ANPRCRowStyle.Normal,
            Activate = () =>
            {
                var level = state.Volume >= ANPRCRadioComponent.MaxVolume ? 0 : state.Volume + 1;
                Radio.SetVolume(level);
                Host.Acknowledge("SPEAKER " + ANPRCVolume.Label(level));
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "BACKLIGHT",
            Value = "LT",
            Activate = () => Radio.CycleBacklight(),
        });

        // ----- station ---------------------------------------------------------------------------

        rows.Add(ANPRCScreenRow.Heading("STATION"));

        var station = !string.IsNullOrEmpty(state.Callsign)
            ? state.Callsign
            : !string.IsNullOrEmpty(state.WearerCallsign)
                ? state.WearerCallsign + " AUTO"
                : "UNKNOWN";

        rows.Add(new ANPRCScreenRow
        {
            Label = "CALLSIGN",
            Value = station,
            Style = string.IsNullOrEmpty(state.Callsign) && string.IsNullOrEmpty(state.WearerCallsign)
                ? ANPRCRowStyle.Warn
                : ANPRCRowStyle.Normal,
            Activate = () => Host.BeginEntry(new ANPRCEntry
            {
                Prompt = "STATION",
                Mode = ANPRCEntryMode.Letters,
                MaxLength = ANPRCRadioComponent.MaxCallsignLength,
                Initial = state.Callsign,
                Commit = callsign =>
                {
                    Radio.SetCallsign(callsign);
                    Host.Acknowledge(string.IsNullOrEmpty(callsign) ? "STATION CLEARED" : "STATION " + callsign);
                },
            }),
        });

        if (!string.IsNullOrEmpty(state.Callsign))
        {
            rows.Add(new ANPRCScreenRow
            {
                Label = "CLEAR OVERRIDE",
                Value = "AUTO",
                Activate = () =>
                {
                    Radio.SetCallsign(string.Empty);
                    Host.Acknowledge("STATION CLEARED");
                },
            });
        }

        if (state.CallsignPresets.Count > 0)
        {
            rows.Add(new ANPRCScreenRow
            {
                Label = "ROSTER",
                Value = ">",
                Activate = () => Host.Push(new ANPRCCallsignPresetScreen()),
            });
        }

        rows.Add(new ANPRCScreenRow
        {
            Label = "NET DIRECTORY",
            Value = state.HasDirectory ? ">" : "NONE",
            Style = state.HasDirectory ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = state.HasDirectory ? () => Radio.OpenDirectory() : null,
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "RADIO CHECK",
            Value = context.Ready ? "SEND" : !context.Deployed ? "STOWED" : "NO NET",
            Style = context.Ready ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = context.Ready
                ? () =>
                {
                    Radio.RadioCheck();
                    Host.Acknowledge("RADIO CHECK SENT");
                }
                : null,
        });

        // ----- what the set is ------------------------------------------------------------------

        rows.Add(ANPRCScreenRow.Heading("SET STATUS"));

        rows.Add(ANPRCScreenRow.Info("ROLE", state.Planted ? "RETRANS" : "MANPACK"));
        rows.Add(ANPRCScreenRow.Info("ANTENNA", state.AntennaLabel));

        var anchoring = context.Online && context.HasActiveNet && !state.SweepEnabled;

        rows.Add(ANPRCScreenRow.Info(
            "RELAY",
            anchoring ? "ANCHOR ACTIVE" : "STANDBY",
            anchoring ? ANPRCRowStyle.Good : ANPRCRowStyle.Note));

        rows.Add(ANPRCScreenRow.Info(
            "AUDIO",
            state.HandsetOut ? "HANDSET" : state.Volume <= 0 ? "MUTED" : ANPRCVolume.Label(state.Volume),
            state.Volume <= 0 && !state.HandsetOut ? ANPRCRowStyle.Warn : ANPRCRowStyle.Normal));

        if (!context.Deployed)
            rows.Add(ANPRCScreenRow.Note("WEAR OR STAKE THE SET TO WORK A NET"));
    }
}

/// <summary>The faction's roster of callsigns, for a station that should answer as one of them.</summary>
public sealed class ANPRCCallsignPresetScreen : ANPRCScreen
{
    public override string Title => "OPT  ROSTER";

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var presets = context.State.CallsignPresets;

        if (presets.Count == 0)
        {
            rows.Add(ANPRCScreenRow.Note("NO ROSTER HELD"));
            return;
        }

        foreach (var preset in presets)
        {
            var captured = preset;

            rows.Add(new ANPRCScreenRow
            {
                Label = captured,
                Activate = () =>
                {
                    Radio.SetCallsign(captured);
                    Host.Acknowledge("STATION " + captured);
                    Host.Pop();
                },
            });
        }
    }
}
