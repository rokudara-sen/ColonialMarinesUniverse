using Robust.Shared.Timing;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     SEC: the fill the set is holding and the three things that can be done to it. None of them
///     can be walked back, so each arms on the first press and fires on the second.
/// </summary>
public sealed class ANPRCComsecScreen(IGameTiming timing) : ANPRCScreen
{
    private static readonly TimeSpan ArmWindow = TimeSpan.FromSeconds(4);

    private enum ANPRCFillAction : byte
    {
        None,
        Zeroize,
        Destroy,
        Recrypto,
    }

    private ANPRCFillAction _armed;
    private TimeSpan _armedUntil;

    /// <summary>Where the function switch is resting, printed so a pull to LD or Z leaves a trace.</summary>
    public ANPRCKnobPosition Switch { get; set; } = ANPRCKnobPosition.Off;

    public override string Title => "SEC  COMSEC";

    public override string Status(ANPRCPanelContext context)
    {
        var state = context.State;
        var hasFill = !string.IsNullOrEmpty(state.CryptoFaction);

        return hasFill ? state.CryptoStale ? "SUPERSEDED" : "SECURED" : "UNSECURED";
    }

    /// <summary>
    ///     Throwing the switch to Z arms the wipe from the hardware rather than from the screen,
    ///     which is what the detent is for. Reports whether there was anything to arm.
    /// </summary>
    public bool ArmZeroize(ANPRCPanelContext context)
    {
        if (!context.Powered || string.IsNullOrEmpty(context.State.CryptoFaction))
            return false;

        _armed = ANPRCFillAction.Zeroize;
        _armedUntil = timing.CurTime + ArmWindow;

        return true;
    }

    private bool IsArmed(ANPRCFillAction action)
        => _armed == action && timing.CurTime <= _armedUntil;

    private void Arm(ANPRCFillAction action, string armedText, Action commit)
    {
        if (IsArmed(action))
        {
            _armed = ANPRCFillAction.None;
            commit();

            return;
        }

        _armed = action;
        _armedUntil = timing.CurTime + ArmWindow;

        Host.Acknowledge(armedText);
    }

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;
        var hasFill = !string.IsNullOrEmpty(state.CryptoFaction);
        var secured = hasFill && !state.CryptoStale;

        // the arming window runs on the clock, so it is checked as the rows are built rather than
        // left to whatever pressed a key last
        if (_armed != ANPRCFillAction.None && timing.CurTime > _armedUntil)
            _armed = ANPRCFillAction.None;

        rows.Add(ANPRCScreenRow.Info(
            "SWITCH",
            Switch switch
            {
                ANPRCKnobPosition.Load => "LD - FILL WORK",
                ANPRCKnobPosition.Zeroize => "Z - WIPE READY",
                ANPRCKnobPosition.Off => "OFF",
                _ => ANPRCModeKnob.Legend(Switch) + " - OPERATING",
            },
            Switch switch
            {
                ANPRCKnobPosition.Zeroize => ANPRCRowStyle.Bad,
                ANPRCKnobPosition.Load => ANPRCRowStyle.Warn,
                ANPRCKnobPosition.Off => ANPRCRowStyle.Note,
                _ => ANPRCRowStyle.Good,
            }));

        rows.Add(ANPRCScreenRow.Info(
            "FILL",
            hasFill ? state.CryptoDesignation : "NONE",
            secured ? ANPRCRowStyle.Good : hasFill ? ANPRCRowStyle.Warn : ANPRCRowStyle.Bad));

        if (hasFill)
        {
            rows.Add(ANPRCScreenRow.Info("ISSUED TO", state.CryptoFaction.ToUpperInvariant()));

            if (state.CryptoStale)
                rows.Add(ANPRCScreenRow.Note("SUPERSEDED - RECRYPTO REQUIRED"));
        }
        else
        {
            rows.Add(ANPRCScreenRow.Note("INSERT A FILL CARD TO LOAD"));
        }

        rows.Add(ANPRCScreenRow.Info(
            "TRAFFIC",
            secured ? "ENCRYPTED" : "IN CLEAR",
            secured ? ANPRCRowStyle.Good : ANPRCRowStyle.Warn));

        rows.Add(ANPRCScreenRow.Heading("FILL ACTIONS"));

        rows.Add(new ANPRCScreenRow
        {
            Label = IsArmed(ANPRCFillAction.Zeroize) ? "CONFIRM WIPE" : "ZEROIZE",
            Value = hasFill ? "EJECT" : "NO FILL",
            Armed = IsArmed(ANPRCFillAction.Zeroize),
            Style = hasFill ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = context.Powered && hasFill
                ? () => Arm(ANPRCFillAction.Zeroize, "ZEROIZE ARMED - ENT AGAIN TO WIPE", () =>
                {
                    Radio.CryptoZeroize();
                    Host.Acknowledge("FILL ZEROIZED");
                })
                : null,
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = IsArmed(ANPRCFillAction.Destroy) ? "CONFIRM BURN" : "DESTROY",
            Value = hasFill ? "BURN" : "NO FILL",
            Armed = IsArmed(ANPRCFillAction.Destroy),
            Style = hasFill ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = context.Powered && hasFill
                ? () => Arm(ANPRCFillAction.Destroy, "DESTROY ARMED - ENT AGAIN TO BURN", () =>
                {
                    Radio.CryptoDestroy();
                    Host.Acknowledge("FILL DESTROYED");
                })
                : null,
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = IsArmed(ANPRCFillAction.Recrypto) ? "CONFIRM RECRYPTO" : "RECRYPTO",
            Value = secured ? "SUPERSEDE" : "NEEDS FILL",
            Armed = IsArmed(ANPRCFillAction.Recrypto),
            Style = secured ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = context.Powered && secured
                ? () => Arm(ANPRCFillAction.Recrypto, "RECRYPTO ARMED - ENT AGAIN TO ORDER", () =>
                {
                    Radio.CryptoRecrypto();
                    Host.Acknowledge("RECRYPTO ORDERED");
                })
                : null,
        });

        rows.Add(ANPRCScreenRow.Note("RECRYPTO NEEDS COMMAND AUTHORITY"));
        rows.Add(ANPRCScreenRow.Note("IT SUPERSEDES EVERY CARD THE FACTION HOLDS"));
    }
}
