using Rewired;

public static class MapEditorInput
{
    public static Player RewiredPlayer { get { if (_p == null) _p = ReInput.players.GetPlayer(PLAYER_INDEX); return _p; } }

    public static bool Start { get { return RewiredPlayer.GetButtonDown(START); } }
    public static bool Confirm { get { return RewiredPlayer.GetButtonDown(CONFIRM); } }
    public static bool ConfirmHeld { get { return RewiredPlayer.GetButton(CONFIRM); } }
    public static bool Action { get { return RewiredPlayer.GetButtonDown(ACTION); } }
    public static bool SwapModes { get { return RewiredPlayer.GetButtonDown(MODE_SWAP); } }
    public static bool Cancel { get { return RewiredPlayer.GetButtonDown(CANCEL); } }
    public static bool Exit { get { return RewiredPlayer.GetButtonDown(EXIT); } }

    public static bool NavLeft { get { return RewiredPlayer.GetButtonDown(NAV_LEFT) || RewiredPlayer.GetButtonShortPress(NAV_LEFT); } }
    public static bool NavRight { get { return RewiredPlayer.GetButtonDown(NAV_RIGHT) || RewiredPlayer.GetButtonShortPress(NAV_RIGHT); } }
    public static bool NavDown { get { return RewiredPlayer.GetButtonDown(NAV_DOWN) || RewiredPlayer.GetButtonShortPress(NAV_DOWN); } }
    public static bool NavUp { get { return RewiredPlayer.GetButtonDown(NAV_UP) || RewiredPlayer.GetButtonShortPress(NAV_UP); } }

    public static bool ResizeLeft { get { return RewiredPlayer.GetButtonDown(RESIZE_LEFT) || RewiredPlayer.GetButtonShortPress(RESIZE_LEFT); } }
    public static bool ResizeRight { get { return RewiredPlayer.GetButtonDown(RESIZE_RIGHT) || RewiredPlayer.GetButtonShortPress(RESIZE_RIGHT); } }
    public static bool ResizeDown { get { return RewiredPlayer.GetButtonDown(RESIZE_DOWN) || RewiredPlayer.GetButtonShortPress(RESIZE_DOWN); } }
    public static bool ResizeUp { get { return RewiredPlayer.GetButtonDown(RESIZE_UP) || RewiredPlayer.GetButtonShortPress(RESIZE_UP); } }

    public static bool CyclePrev { get { return RewiredPlayer.GetButtonDown(CYCLE_PREV) || RewiredPlayer.GetButtonShortPress(CYCLE_PREV); } }
    public static bool CycleNext { get { return RewiredPlayer.GetButtonDown(CYCLE_NEXT) || RewiredPlayer.GetButtonShortPress(CYCLE_NEXT); } }
    public static bool CyclePrevAlt { get { return RewiredPlayer.GetButtonDown(CYCLE_PREV_ALT) || RewiredPlayer.GetButtonShortPress(CYCLE_PREV_ALT); } }
    public static bool CycleNextAlt { get { return RewiredPlayer.GetButtonDown(CYCLE_NEXT_ALT) || RewiredPlayer.GetButtonShortPress(CYCLE_NEXT_ALT); } }

    /**
     * Private
     */
    private static int PLAYER_INDEX = 0;
    private static string START = "Start";
    private static string CONFIRM = "Confirm";
    private static string ACTION = "Action";
    private static string MODE_SWAP = "ModeSwap";
    private static string CANCEL = "Cancel";
    private static string EXIT = "Exit";
    private static string NAV_LEFT = "NavLeft";
    private static string NAV_RIGHT = "NavRight";
    private static string NAV_DOWN = "NavDown";
    private static string NAV_UP = "NavUp";
    private static string RESIZE_LEFT = "ResizeLeft";
    private static string RESIZE_RIGHT = "ResizeRight";
    private static string RESIZE_DOWN = "ResizeDown";
    private static string RESIZE_UP = "ResizeUp";
    private static string CYCLE_PREV = "CyclePrev";
    private static string CYCLE_NEXT = "CycleNext";
    private static string CYCLE_PREV_ALT = "CyclePrev2";
    private static string CYCLE_NEXT_ALT = "CycleNext2";
    private static Player _p;
}
