using System.Linq;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     SRCH: the search receiver and what it has fixed. Walking the band takes the set off every
///     net it holds, so the screen says so on its own line rather than burying it.
/// </summary>
public sealed class ANPRCSearchScreen : ANPRCScreen
{
    public override string Title => "SRCH  RECEIVER";

    public override string Status(ANPRCPanelContext context)
    {
        if (!context.Deployed)
            return "STOWED";

        return context.State.SweepEnabled ? "SEARCHING" : "IDLE";
    }

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;
        var searching = state.SweepEnabled && context.Online;

        rows.Add(new ANPRCScreenRow
        {
            Label = searching ? "STOP SEARCH" : "START SEARCH",
            Value = !context.Deployed ? "STOWED" : searching ? "RUNNING" : "READY",
            Lit = searching,
            Style = searching ? ANPRCRowStyle.Warn : ANPRCRowStyle.Normal,
            // the receiver is the one thing here that needs an antenna in the air, and the server
            // refuses to start a sweep off a pack that is neither worn nor staked down
            Activate = context.Online
                ? () =>
                {
                    Radio.SetSweep(!state.SweepEnabled);
                    Host.Acknowledge(state.SweepEnabled ? "SEARCH STOPPED" : "SEARCH RUNNING");
                }
                : null,
        });

        rows.Add(ANPRCScreenRow.Info(
            "HEAD",
            searching ? ANPRCPanelContext.FormatFrequency(state.SweepPosition) : "---.---",
            searching ? ANPRCRowStyle.Good : ANPRCRowStyle.Note));

        rows.Add(searching
            ? ANPRCScreenRow.Info("STATE", "NETS DROPPED, TX INHIBITED", ANPRCRowStyle.Warn)
            : ANPRCScreenRow.Note(context.Deployed
                ? "SEARCHING DROPS EVERY NET"
                : "WEAR OR STAKE THE SET TO WALK THE BAND"));

        rows.Add(ANPRCScreenRow.Heading("CONTACTS"));

        if (state.SweepContacts.Count == 0)
        {
            rows.Add(ANPRCScreenRow.Note("NO CONTACTS"));
            return;
        }

        var slot = state.ActiveSlot;
        var canTune = slot >= 0 && state.SlotLabels.ContainsKey(slot);

        foreach (var contact in state.SweepContacts)
        {
            if (!contact.Resolved)
            {
                var masked = ANPRCPanelContext.MaskDigits(
                    ANPRCPanelContext.FormatFrequency(contact.Frequency),
                    contact.Tier,
                    contact.TierMax);

                rows.Add(ANPRCScreenRow.Info(
                    "~" + masked,
                    "PARTIAL " + contact.Tier + "/" + contact.TierMax,
                    ANPRCRowStyle.Warn));

                continue;
            }

            var frequency = ANPRCPanelContext.FormatFrequency(contact.Frequency);
            var name = contact.ChannelName.ToUpperInvariant();

            // an own net was never work. it is listed so the band reads honestly, dimmed so it
            // never looks like something the operator won
            if (contact.Known)
            {
                rows.Add(ANPRCScreenRow.Info(frequency, name + " (OWN)", ANPRCRowStyle.Note));
                continue;
            }

            var captured = contact.Frequency;

            rows.Add(new ANPRCScreenRow
            {
                Label = frequency,
                Value = canTune ? name : name + " (NO MEM)",
                Style = ANPRCRowStyle.Good,
                Activate = canTune
                    ? () =>
                    {
                        Radio.TuneContact(slot, captured);
                        Host.Acknowledge("CONTACT TUNED");
                    }
                    : null,
            });
        }

        if (!canTune && state.SweepContacts.Any(contact => contact.Resolved && !contact.Known))
            rows.Add(ANPRCScreenRow.Note("SELECT A MEMORY ON PGM TO TUNE A FIX"));
    }
}
