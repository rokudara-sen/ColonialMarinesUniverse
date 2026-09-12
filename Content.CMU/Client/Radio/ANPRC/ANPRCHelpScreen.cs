namespace Content.Client.CMU14.Radio.ANPRC;

/// <summary>
///     The laminated card taped inside the lid of a real set. The panel is modelled on hardware
///     most people have never touched and is worked from the keypad, so the legends have to be
///     readable without leaving the radio.
/// </summary>
public sealed class ANPRCHelpScreen : ANPRCScreen
{
    public override string Title => "?  KEY CARD";

    public override string Status(ANPRCPanelContext context)
        => !context.Powered ? "SET OFF" : context.Deployed ? "ONLINE" : "STOWED";

    public override void Build(ANPRCPanelContext context, List<ANPRCScreenRow> rows)
    {
        rows.Add(ANPRCScreenRow.Heading("WORKING THE SCREEN"));
        rows.Add(ANPRCScreenRow.Info("+/- PRE", "MOVE THE CURSOR"));
        rows.Add(ANPRCScreenRow.Info("ENT", "WORK THE LINE UNDER IT"));
        rows.Add(ANPRCScreenRow.Info("CLR", "BACK OUT ONE SCREEN"));
        rows.Add(ANPRCScreenRow.Info("+/- VOL", "SPEAKER"));
        rows.Add(ANPRCScreenRow.Note("THE SOFT KEYS BELOW PAGE THE SCREEN"));

        rows.Add(ANPRCScreenRow.Heading("KEYPAD"));
        rows.Add(ANPRCScreenRow.Info("1 CALL", "RADIO CHECK"));
        rows.Add(ANPRCScreenRow.Info("2 LT", "BACKLIGHT"));
        rows.Add(ANPRCScreenRow.Info("3 MODE", "WAVEFORM"));
        rows.Add(ANPRCScreenRow.Info("4 SQL", "SQUELCH"));
        rows.Add(ANPRCScreenRow.Info("5 ZERO", "COMSEC, WIPE ARMED"));
        rows.Add(ANPRCScreenRow.Info("6 PWR", "SET ON AND OFF"));
        rows.Add(ANPRCScreenRow.Info("7 8 9 0", "OPT PGM SEC LOG"));

        rows.Add(ANPRCScreenRow.Heading("KEYING NUMBERS AND TEXT"));
        rows.Add(ANPRCScreenRow.Note("A FIELD TAKES THE PAD WHEN YOU OPEN IT."));
        rows.Add(ANPRCScreenRow.Note("FREQUENCIES GO IN AGAINST ___.___ , MHZ"));
        rows.Add(ANPRCScreenRow.Note("FIRST. UNTOUCHED DIGITS READ AS ZERO."));
        rows.Add(ANPRCScreenRow.Note("TEXT USES THE LETTERS OVER EACH KEY -"));
        rows.Add(ANPRCScreenRow.Note("PRESS AGAIN TO STEP A B C. THE CAPS SHOW"));
        rows.Add(ANPRCScreenRow.Note("EACH KEY'S GROUP WHILE YOU TYPE."));
        rows.Add(ANPRCScreenRow.Note("0 KEYS A SPACE, THEN A DASH."));
        rows.Add(ANPRCScreenRow.Note("CLR RUBS OUT, ENT SETS IT."));

        rows.Add(ANPRCScreenRow.Heading("FUNCTION SWITCH"));
        rows.Add(ANPRCScreenRow.Info("OFF", "SET DOWN"));
        rows.Add(ANPRCScreenRow.Info("CT", "SECURE - AGAIN STEPS FH SC CT"));
        rows.Add(ANPRCScreenRow.Info("PT", "IN CLEAR, ANYONE READS YOU"));
        rows.Add(ANPRCScreenRow.Info("LD", "FILL PAGE. SPRUNG"));
        rows.Add(ANPRCScreenRow.Info("Z", "ARMS THE WIPE. SPRUNG"));
        rows.Add(ANPRCScreenRow.Note("LD AND Z ARE SPRUNG PULLS. THE SWITCH"));
        rows.Add(ANPRCScreenRow.Note("RETURNS ON ITS OWN AND YOUR WAVEFORM"));
        rows.Add(ANPRCScreenRow.Note("IS NOT TOUCHED. THAT IS THE DETENT."));

        rows.Add(ANPRCScreenRow.Heading("PROCEDURE"));
        rows.Add(ANPRCScreenRow.Note("THE PANEL WORKS OFF ANY SWITCHED-ON SET,"));
        rows.Add(ANPRCScreenRow.Note("IN THE HAND OR ON THE GROUND. WEAR IT OR"));
        rows.Add(ANPRCScreenRow.Note("STAKE IT DOWN TO WORK A NET."));
        rows.Add(ANPRCScreenRow.Note("ADD A MEMORY ON PGM, TUNE IT, SELECT IT."));
        rows.Add(ANPRCScreenRow.Note("SPEAK WITH :r ON THE WORKING MEMORY."));
        rows.Add(ANPRCScreenRow.Note("NO FILL MEANS YOUR TRAFFIC GOES IN CLEAR."));
    }
}
