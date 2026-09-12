using System.Linq;
using Content.Shared.CMU14.Radio;

namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     LOG: the set's net log, fifty lines and rolling. Each entry is two lines - who and where on
///     the first, what they said on the second - because a transcript that has to fit one row of a
///     character grid is not a transcript.
/// </summary>
public sealed class ANPRCLogScreen : ANPRCScreen
{
    /// <summary>null lists every net; otherwise only traffic on this one.</summary>
    private string? _filter;

    private bool _interceptsOnly;

    public override string Title => "LOG  NET LOG";

    public override string Status(ANPRCPanelContext context)
        => context.State.NetLog.Count + "/" + ANPRCRadioComponent.MaxNetLogEntries;

    private IEnumerable<ANPRCNetLogEntry> Matching(ANPRCPanelContext context)
    {
        return context.State.NetLog.Where(entry =>
            (!_interceptsOnly || entry.Intercepted) &&
            (_filter == null || entry.ChannelDisplay == _filter));
    }

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        var state = context.State;

        var nets = state.NetLog
            .Select(entry => entry.ChannelDisplay)
            .Distinct()
            .OrderBy(net => net, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // a filter left pointing at a net that has rolled out of the fifty-line log would hide
        // everything with no way to tell why
        if (_filter != null && !nets.Contains(_filter))
            _filter = null;

        rows.Add(new ANPRCScreenRow
        {
            Label = "NET FILTER",
            Value = _filter ?? "ALL NETS",
            Lit = _filter != null,
            Activate = () =>
            {
                if (nets.Count == 0)
                {
                    _filter = null;
                    return;
                }

                var index = _filter == null ? -1 : nets.IndexOf(_filter);

                // steps through the nets in the log and then back to all of them
                _filter = index + 1 >= nets.Count ? null : nets[index + 1];
                Host.Refresh();
            },
        });

        rows.Add(new ANPRCScreenRow
        {
            Label = "INTERCEPTS ONLY",
            Value = _interceptsOnly ? "ON" : "OFF",
            Lit = _interceptsOnly,
            Activate = () =>
            {
                _interceptsOnly = !_interceptsOnly;
                Host.Refresh();
            },
        });

        var printable = context.Online && state.NetLog.Count > 0;

        rows.Add(new ANPRCScreenRow
        {
            Label = "PRINT LOG",
            Value = context.Deployed ? "PAPER" : "STOWED",
            Style = printable ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = printable
                ? () =>
                {
                    Radio.PrintLog(false);
                    Host.Acknowledge("LOG PRINTED");
                }
                : null,
        });

        var interceptable = context.Online && state.NetLog.Any(entry => entry.Intercepted);

        rows.Add(new ANPRCScreenRow
        {
            Label = "PRINT INTERCEPTS",
            Value = !context.Deployed ? "STOWED" : interceptable ? "PAPER" : "NONE",
            Style = interceptable ? ANPRCRowStyle.Normal : ANPRCRowStyle.Note,
            Activate = interceptable
                ? () =>
                {
                    Radio.PrintLog(true);
                    Host.Acknowledge("INTERCEPTS PRINTED");
                }
                : null,
        });

        rows.Add(ANPRCScreenRow.Heading("TRAFFIC"));

        if (state.NetLog.Count == 0)
        {
            rows.Add(ANPRCScreenRow.Note("LOG EMPTY"));
            return;
        }

        var shown = 0;

        // newest at the top: an operator opening the log wants what just came over the air
        foreach (var entry in Matching(context).Reverse())
        {
            shown++;

            var stamp = TimeSpan.FromSeconds(entry.Timestamp);
            var time = ((int) stamp.TotalMinutes).ToString("D2") + ":" + stamp.Seconds.ToString("D2");

            rows.Add(ANPRCScreenRow.Info(
                time + " " + entry.SenderName,
                entry.ChannelDisplay + (entry.Intercepted ? " INT" : string.Empty),
                entry.Intercepted ? ANPRCRowStyle.Warn : ANPRCRowStyle.Normal));

            rows.Add(ANPRCScreenRow.Note("  " + entry.Message));
        }

        if (shown == 0)
            rows.Add(ANPRCScreenRow.Note("NO TRAFFIC MATCHES THE FILTER"));
    }
}
