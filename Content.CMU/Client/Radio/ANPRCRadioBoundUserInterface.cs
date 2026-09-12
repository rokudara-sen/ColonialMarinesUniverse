using Content.Client._RMC14.UserInterface;
using Content.Client.CMU14.Radio.ANPRC;
using Content.Shared.CMU14.Radio;

namespace Content.Client.CMU14.Radio;

public sealed class ANPRCRadioBoundUserInterface : RMCPopOutBui<ANPRCRadioWindow>
{
    [ViewVariables]
    protected override ANPRCRadioWindow? Window { get; set; }

    public ANPRCRadioBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        // the set can be lifted out into an OS window of its own, the way the tactical map is:
        // an operator works the panel with one hand and the rest of the game with the other
        var window = Window = this.CreatePopOutableWindow<ANPRCRadioWindow>();

        window.OnSelectSlot += slot => SendMessage(new ANPRCSelectSlotMsg(slot));
        window.OnTogglePower += () => SendMessage(new ANPRCTogglePowerMsg());
        window.OnToggleMonitor += () => SendMessage(new ANPRCToggleMonitorMsg());
        window.OnSetMode += mode => SendMessage(new ANPRCSetModeMsg(mode));
        window.OnSetScan += enabled => SendMessage(new ANPRCSetScanMsg(enabled));
        window.OnSetTxPower += power => SendMessage(new ANPRCSetTxPowerMsg(power));
        window.OnSetSquelch += level => SendMessage(new ANPRCSetSquelchMsg(level));
        window.OnSetVolume += level => SendMessage(new ANPRCSetVolumeMsg(level));
        window.OnSetCallsign += callsign => SendMessage(new ANPRCSetCallsignMsg(callsign));
        window.OnAddSlot += label => SendMessage(new ANPRCAddSlotMsg(label));
        window.OnDeleteSlot += slot => SendMessage(new ANPRCDeleteSlotMsg(slot));
        window.OnSetSlotChannel += (s, ch) => SendMessage(new ANPRCSetSlotChannelMsg(s, ch));
        window.OnClearSlot += slot => SendMessage(new ANPRCClearSlotMsg(slot));
        window.OnCryptoZeroize += () => SendMessage(new ANPRCCryptoZeroizeMsg());
        window.OnCryptoDestroy += () => SendMessage(new ANPRCCryptoDestroyMsg());
        window.OnCryptoRecrypto += () => SendMessage(new ANPRCCryptoRecryptoMsg());
        window.OnRadioCheck += () => SendMessage(new ANPRCRadioCheckMsg());
        window.OnOpenDirectory += () => SendMessage(new ANPRCOpenDirectoryMsg());
        window.OnManualFrequency += (slot, text) => SendMessage(new ANPRCManualFrequencyMsg(slot, text));
        window.OnSetSweep += enabled => SendMessage(new ANPRCSetSweepMsg(enabled));
        window.OnTuneContact += (slot, freq) => SendMessage(new ANPRCTuneContactMsg(slot, freq));
        window.OnPrintLog += intercepts => SendMessage(new ANPRCPrintLogMsg(intercepts));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not ANPRCRadioState s)
            return;

        Window?.UpdateState(s);
    }
}
