using UnityEngine;

public class SimplePauser : MonoBehaviour
{
    public bool Editor = false;

    void Update()
    {
        if (!Editor ? GameplayInput.PausePressed : MapEditorInput.SwapModes)
        {
            _paused = !_paused;
            if (_paused)
                PauseController.UserPause();
            else
                PauseController.UserResume();
        }
    }

    private bool _paused = false;
}
