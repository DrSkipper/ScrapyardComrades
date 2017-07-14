using UnityEngine;

public class SimplePauser : MonoBehaviour
{
    public bool Editor = false;
    public bool MainMenu = false;

    private delegate bool InputCheckDelegate();
    private InputCheckDelegate _inputCheck;

    void Awake()
    {
        if (this.Editor)
            _inputCheck = editorInputCheck;
        else if (this.MainMenu)
            _inputCheck = mainMenuInputCheck;
        else
            _inputCheck = gameplayInputCheck;
    }

    void Update()
    {
        if (_inputCheck())
        {
            _paused = !_paused;
            if (_paused)
                PauseController.UserPause();
            else
                PauseController.UserResume();
        }
    }

    private bool _paused = false;

    private bool gameplayInputCheck()
    {
        return GameplayInput.PausePressed;
    }

    private bool editorInputCheck()
    {
        return MapEditorInput.SwapModes;
    }

    private bool mainMenuInputCheck()
    {
        return (!_paused && GameplayInput.AttackLightHeld) || (_paused && !GameplayInput.AttackLightHeld);
    }
}
