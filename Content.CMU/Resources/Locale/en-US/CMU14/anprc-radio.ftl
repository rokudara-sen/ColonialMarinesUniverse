anprc-window-title = AN/PRC-117G Tactical Radio

anprc-transmit-hint-header = TRANSMIT
anprc-transmit-hint-active = :r transmits on the active preset

anprc-power-off-button = POWER OFF
anprc-power-on-button = POWER ON

anprc-status-equipped = EQUIPPED
anprc-status-unequipped = UNEQUIPPED
anprc-status-on = ON
anprc-status-off = OFF

anprc-slot-empty-display = NO CHANNEL


anprc-radio-off = The radio is silent. It is switched off.
anprc-not-authorized = The controls refuse to respond. You are not trained to operate this radio.
anprc-no-active-slot = No preset is selected. Load and select a net first.
anprc-slot-empty = Preset { $slot } is not tuned to anything.
anprc-no-tower = Static. { $channel } has no communications relay in range.
anprc-not-rto-warning = You are not trained to operate this radio. While you carry it, it cannot relay nets and you cannot transmit through it.
anprc-verb-open = Open Radio Panel
anprc-out-of-range = Static. No relay in range for { $channel }.

anprc-frequency-invalid = Invalid frequency. Enter it as 1606 or 1.606.
anprc-frequency-out-of-band = Frequency out of range. Direct frequencies must be within 1.000-2.999 MHz or the 30.000-87.999 MHz softwave band.
anprc-frequency-not-found = Nothing is assigned to { $freq } MHz.
anprc-frequency-set = [{ $slot }] tuned to { $freq } MHz.
anprc-frequency-set-net = [{ $slot }] tuned to { $freq } MHz — { $channel }.
anprc-frequency-set-unknown = [{ $slot }] tuned to { $freq } MHz — unidentified net. Received traffic will be logged.
anprc-frequency-set-dynamic = [{ $slot }] tuned to { $freq } MHz — direct frequency. Transmit with :r.

anprc-frequency-card-fallback =
    {"["}head=2]SIGNAL OPERATING INSTRUCTIONS[/head]
    {"["}head=3]AN/PRC-117G NET FREQUENCY ASSIGNMENTS[/head]

    {"["}bold]GOVFOR Command[/bold] - 2.592 MHz
    {"["}bold]GOVFOR Alpha[/bold]   - 2.502 MHz
    {"["}bold]GOVFOR Bravo[/bold]   - 1.606 MHz
    {"["}bold]GOVFOR Charlie[/bold] - 1.607 MHz
    {"["}bold]GOVFOR Intel[/bold]   - 1.605 MHz
    {"["}bold]GOVFOR JTAC[/bold]    - 2.598 MHz
    {"["}bold]GOVFOR MILP[/bold]    - 2.595 MHz

    Enter a frequency through the radio's FREQ page to tune a preset. The decimal may be omitted: 2592 and 2.592 both tune 2.592 MHz.

    {"["}italic]COMSEC NOTICE: Controlled material. Destroy before capture. Frequency assignments are theatre-wide and should be considered compromised if this card is lost.[/italic]

anprc-slot-max-reached = All four preset slots are in use. Delete one before adding another.

anprc-monitor-no-transmit = MONITOR ACTIVE — transmit disabled. Turn MON off to transmit.

anprc-ct-mode-no-fill = CT MODE — no crypto fill loaded. Insert a valid COMSEC fill before transmitting.

anprc-scan-switched = SCAN — traffic received on [{ $label }] (P{ $slot } · { $channel }). Preset selected.

anprc-squelch-suppressed = *squelch*

anprc-volume-muted = SPEAKER MUTE — radio audio muted. The handset remains available.
anprc-volume-restored = SPEAKER { $level } — speaker enabled.
anprc-volume-loud = SPEAKER { $level } — radio traffic is audible up to { $range } tiles away.
anprc-volume-quiet = SPEAKER { $level } — audio limited to the operator.

anprc-crypto-not-equipped = The radio must be worn before a crypto fill can be loaded.
anprc-crypto-already-loaded = { $designation } is already loaded. Zeroize it before inserting another fill.
anprc-crypto-loaded = { $designation } loaded. Secure traffic available.
anprc-crypto-zeroized = { $designation } zeroized and ejected. Secure traffic unavailable.
anprc-crypto-destroyed = { $designation } destroyed. The fill cannot be recovered.
anprc-crypto-no-card = No crypto fill loaded.
anprc-crypto-wrong-faction = This fill is not compatible with the radio.
anprc-crypto-examine-empty = No crypto fill loaded. Secure traffic is unavailable.
anprc-crypto-examine-loaded = { $designation } loaded ({ $faction }).
anprc-crypto-examine-stale = { $designation } loaded ({ $faction }) — SUPERSEDED. This fill no longer secures current traffic.

anprc-comsec-unsecured = COMSEC WARNING: No valid fill for { $channel } ({ $faction }). Transmission will be sent in the clear.

anprc-recrypto-no-card = No valid fill loaded. Insert a current fill for your faction first.
anprc-recrypto-stale-card = The loaded fill has already been superseded. Insert a current fill before ordering recrypto.
anprc-recrypto-foreign-card = CHANGEOVER DENIED — loaded fill does not match this radio's issuing authority.
anprc-recrypto-ordered = COMSEC CHANGEOVER ORDERED — all earlier { $faction } fills are now superseded. Obtain replacement fill through normal resupply.
anprc-recrypto-not-authorized = CHANGEOVER DENIED — command COMSEC authority required.
anprc-recrypto-button = ORDER RECRYPTO — SUPERSEDE FACTION FILL
anprc-recrypto-button-confirm = CONFIRM RECRYPTO — SUPERSEDES ALL CURRENT FACTION FILLS
anprc-recrypto-superseded-notice = COMSEC CHANGEOVER — your loaded fill has been superseded. Obtain a replacement.

anprc-battery-depleted = The battery is completely discharged. Replace it before using the radio.
anprc-battery-empty = The AN/PRC-117G dies as the battery runs flat.
anprc-battery-insufficient = Battery charge too low to transmit.

anprc-unknown-station = UNKNOWN STATION
anprc-radio-check-call = ALL STATIONS, THIS IS { $station }, RADIO CHECK, OVER.
anprc-radio-check-report = RADIO CHECK — LIMA CHARLIE: { $clear } | WEAK BUT READABLE: { $degraded }
anprc-radio-check-nothing-heard = NOTHING HEARD
anprc-radio-check-interference = INTERFERENCE DETECTED — strongest emitter bearing { $bearing }.

anprc-verb-plant = Set Up Retrans
anprc-verb-packup = Pack Up Radio
anprc-retrans-planted = You stake the radio down and bring it online as an unattended retransmission station.
anprc-retrans-packed = You collapse the retransmission station back into a manpack.
anprc-retrans-pickup-blocked = The radio is staked down as a retransmission station. Pack it up first.

anprc-verb-handset = Take Handset
anprc-verb-handset-release = Hang Up Handset
anprc-handset-taken = You lift the corded handset from { $radio }.
anprc-handset-released = You return the handset to { $radio }.
anprc-handset-in-use = Someone else is already using the handset.
anprc-handset-hands-full = You need a free hand to take the handset.
anprc-handset-cord = The handset is pulled from your hand as the cord reaches its limit.
anprc-handset-radio-gone = The handset goes dead.
anprc-handset-hint = While holding the handset, normal speech is transmitted over the radio's active net. Whisper to speak locally.

# Search receiver
anprc-sweep-started = The radio drops off its loaded nets and begins sweeping the band. Receive and transmit are unavailable until the search stops.
anprc-sweep-needs-online = The radio must be powered and deployed before it can search the band.
anprc-sweep-aborted = The search stops as the radio goes offline.
anprc-sweep-aborted-power = The battery dies and the search stops.
anprc-sweep-tx-blocked = SEARCH ACTIVE — stop the band sweep before transmitting.
anprc-sweep-contact = SIGNAL DETECTED — activity near { $freq } MHz.
anprc-sweep-resolved = FIX — { $freq } MHz · { $net }
anprc-sweep-unknown-net = UNIDENTIFIED NET

# Net log to paper
anprc-log-print-empty = The net log is empty.
anprc-log-printed = You transcribe { $count } log entries onto paper.

anprc-log-frequency-unknown = FREQ UNK

# Languages that do not carry over the air
anprc-language-no-radio = { $language } cannot be transmitted over the radio.
