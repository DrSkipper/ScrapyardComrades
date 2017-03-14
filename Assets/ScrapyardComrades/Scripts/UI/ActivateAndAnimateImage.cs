using UnityEngine;
using UnityEngine.UI;

public class ActivateAndAnimateImage : MonoBehaviour
{
    public Image Image;
    public Animation Animation;

    void Start()
    {
        Color c = this.Image.color;
        c.a = 0;
        this.Image.color = c;
    }

    public void Run()
    {
        this.Animation.Stop();
        this.Animation.Play();
    }
}
