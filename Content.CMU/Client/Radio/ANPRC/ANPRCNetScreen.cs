using System.Linq;
using Content.Shared.CMU14.Radio;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     PGM: the net memories the set carries and which one it is working. The screen the panel
///     opens on, because it is where an operator spends the round.
/// </summary>
public sealed class ANPRCNetScreen : ANPRCScreen
{
    public override string Title => "PGM  NET MEMORY";

    public override string Status(ANPRCPanelContext context)
        => context.State.SlotLabels.Count + "/" + ANPRCRadioComponent.MaxSlots;

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;

        if (state.SlotLabels.Count == 0)
            rows.Add(ANPRCScreenRow.Note("NO NETS IN MEMORY"));

        foreach (var slot in state.SlotLabels.Keys.OrderBy(key => key))
        {
            var captured = slot;
            var active = slot == state.ActiveSlot;

            rows.Add(new ANPRCScreenRow
            {
                Label = (active ? "*" : " ") + context.SlotLabel(slot),
                Value = context.SlotContents(slot),
                Lit = active,
                Activate = () => Host.Push(new ANPRCMemoryScreen(captured)),
            });
        }

        if (state.SlotLabels.Count < ANPRCRadioComponent.MaxSlots)
        {
            rows.Add(new ANPRCScreenRow
            {
                Label = "ADD NET",
                Value = "+",
                Activate = () => Host.BeginEntry(new ANPRCEntry
                {
                    Prompt = "LABEL",
                    Mode = ANPRCEntryMode.Letters,
                    MaxLength = ANPRCRadioComponent.MaxLabelLength,
                    Commit = label =>
                    {
                        Radio.AddSlot(label);
                        Host.Acknowledge("MEMORY " + label + " ADDED");
                    },
                }),
            });
        }

        rows.Add(ANPRCScreenRow.Note("ENT WORKS THE MEMORY UNDER THE CURSOR"));
        rows.Add(ANPRCScreenRow.Note("*  IS THE WORKING NET. :r KEYS IT"));
    }
}

/// <summary>
///     What can be done to one memory. A real set puts the verbs on their own page rather than
///     hanging three buttons off every row, and it means the memory list stays readable.
/// </summary>
public sealed class ANPRCMemoryScreen(int slot) : ANPRCScreen
{
    public override string Title => "PGM  MEMORY";

    public override string Status(ANPRCPanelContext context) => context.SlotLabel(slot);

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;

        // deleted out from under the cursor: back out rather than working a memory that is gone
        if (!state.SlotLabels.ContainsKey(slot))
        {
            Host.Pop();
            return;
        }

        rows.Add(ANPRCScreenRow.Info("LOADED", context.SlotContents(slot)));

        rows.Add(new ANPRCScreenRow
        {
            Label = "WORK THIS NET",
            Value = slot == state.ActiveSlot ? "ACTIVE" : string.Empty,
            Lit = slot == state.ActiveSlot,
            Activate = () =>
            {
                Radio.SelectSlot(slot);
                Host.Acknowledge("NET " + context.SlotLabel(slot) + " SELECTED");
                Host.Pop();
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "TUNE TO A NET",
            Value = ">",
            Activate = () => Host.Push(new ANPRCNetListScreen(slot)),
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "KEY A FREQUENCY",
            Value = ">",
            Activate = () => Host.BeginEntry(new ANPRCEntry
            {
                Prompt = "FREQ [" + context.SlotLabel(slot) + "]",
                Mode = ANPRCEntryMode.Frequency,
                MaxLength = 6,
                Commit = text => Radio.ManualFrequency(slot, text),
            }),
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "EMPTY THE MEMORY",
            Activate = () =>
            {
                Radio.ClearSlot(slot);
                Host.Acknowledge("MEMORY EMPTIED");
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "DELETE THE MEMORY",
            Activate = () =>
            {
                Radio.DeleteSlot(slot);
                Host.Acknowledge("MEMORY CLEARED");
                Host.Pop();
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "RENAME",
            Activate = () => Host.BeginEntry(new ANPRCEntry
            {
                Prompt = "LABEL",
                Mode = ANPRCEntryMode.Letters,
                MaxLength = ANPRCRadioComponent.MaxLabelLength,
                Initial = context.SlotLabel(slot),
                Commit = label =>
                {
                    // the set has no rename message: a memory is its label, so it is deleted and
                    // re-added, which loses the slot's contents exactly as re-keying it would
                    Radio.DeleteSlot(slot);
                    Radio.AddSlot(label);
                    Host.Acknowledge("MEMORY " + label);
                    Host.Pop();
                },
            }),
        });
    }
}

/// <summary>The nets this set is entitled to work, as a list the cursor runs down.</summary>
public sealed class ANPRCNetListScreen(int slot) : ANPRCScreen
{
    public override string Title => "PGM  NET LIST";

    public override string Status(ANPRCPanelContext context) => context.SlotLabel(slot);

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;
        var operatorFaction = state.OperatorFaction;
        var listed = 0;

        foreach (var proto in context.Channels().OrderBy(proto => proto.LocalizedName))
        {
            if (proto.Frequency == RadioFrequency.Off)
                continue;

            // own-faction nets always list. a foreign net lists once the search receiver has
            // fixed it, which the server proves by putting its frequency in the state at all
            var ownNet = string.IsNullOrEmpty(operatorFaction) ||
                         string.Equals(proto.Faction, operatorFaction, StringComparison.OrdinalIgnoreCase);

            var discovered = !ownNet &&
                             !string.IsNullOrEmpty(proto.Faction) &&
                             state.ChannelFrequencies.ContainsKey(proto.ID);

            if (!ownNet && !discovered)
                continue;

            var captured = proto.ID;
            var frequency = ANPRCPanelContext.FormatFrequency(context.PlanFrequency(proto));

            rows.Add(new ANPRCScreenRow
            {
                Label = proto.LocalizedName.ToUpperInvariant() + (discovered ? " (INT)" : string.Empty),
                Value = frequency,
                Style = discovered ? ANPRCRowStyle.Warn : ANPRCRowStyle.Normal,
                Activate = () =>
                {
                    Radio.SetSlotChannel(slot, new ProtoId<RadioChannelPrototype>(captured));
                    Host.Acknowledge("NET LOADED");
                    Host.Pop();
                },
            });

            listed++;
        }

        if (listed == 0)
            rows.Add(ANPRCScreenRow.Note("NO NETS HELD - KEY A FREQUENCY"));
    }
}
