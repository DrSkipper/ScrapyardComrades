using Rewired;

public static class MapEditorInput
{
    public static Player RewiredPlayer { get { if (_p == null) _p = ReInput.players.GetPlayer(PLAYER_INDEX); return _p; } }

    public static bool Start { get { return RewiredPlayer.GetButtonDown(START); } }
    public static bool Confirm { get { return RewiredPlayer.GetButtonDown(CONFIRM); } }
    public static bool Cancel { get { return RewiredPlayer.GetButtonDown(CANCEL); } }
    public static bool Exit { get { return RewiredPlayer.GetButtonDown(EXIT); } }

    public static bool NavLeft { get { return RewiredPlayer.GetButtonDown(NAV_LEFT) || RewiredPlayer.GetButtonShortPress(NAV_LEFT); } }
    public static bool NavRight { get { return RewiredPlayer.GetButtonDown(NAV_RIGHT) || RewiredPlayer.GetButtonShortPress(NAV_RIGHT); } }
    public static bool NavDown { get { return RewiredPlayer.GetButtonDown(NAV_DOWN) || RewiredPlayer.GetButtonShortPress(NAV_DOWN); } }
    public static bool NavUp { get { return RewiredPlayer.GetButtonDown(NAV_UP) || RewiredPlayer.GetButtonShortPress(NAV_UP); } }

    public static bool CyclePrev { get { return RewiredPlayer.GetButtonDown(CYCLE_PREV) || RewiredPlayer.GetButtonShortPress(CYCLE_PREV); } }
    public static bool CycleNext { get { return RewiredPlayer.GetButtonDown(CYCLE_NEXT) || RewiredPlayer.GetButtonShortPress(CYCLE_NEXT); } }

    /**
     * Private
     */
    private static int PLAYER_INDEX = 0;
    private static string START = "Start";
    private static string CONFIRM = "Confirm";
    private static string CANCEL = "Cancel";
    private static string EXIT = "Exit";
    private static string NAV_LEFT = "NavLeft";
    private static string NAV_RIGHT = "NavRight";
    private static string NAV_DOWN = "NavDown";
    private static string NAV_UP = "NavUp";
    private static string CYCLE_PREV = "CyclePrev";
    private static string CYCLE_NEXT = "CycleNext";
    private static Player _p;
}
