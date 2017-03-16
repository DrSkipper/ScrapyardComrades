using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleTransitioner : MonoBehaviour
{
    public string ButtonToActivate = "Cancel";
    public string DestinationScene = "WorldEditor";

    void Update()
    {
        if (GameplayInput.ButtonPressed(this.ButtonToActivate))
            SceneManager.LoadScene(this.DestinationScene);
    }
}
