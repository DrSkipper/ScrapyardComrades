using UnityEngine;

public class SimplePauser : MonoBehaviour
{
    void Update()
    {
        if (GameplayInput.PausePressed)
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
