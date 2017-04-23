using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIBar))]
public class LostHealthBarChunk : MonoBehaviour
{
    public UIBar Bar;
    public Image BarImage;
    public Animation Animation;
    public int AnimationDelay;

    void Awake()
    {
        _y = Mathf.RoundToInt(this.Bar.BarTransform.anchoredPosition.y);
        _timer = new Timer(this.AnimationDelay, false, false, playAnimation);
        //this.Animation.clip
    }

    void FixedUpdate()
    {
        _timer.update();
    }

    public void TriggerHealthLostAnimation(int currentHealth, int prevCurrentHealth, int maxHealth, int totalBarLength)
    {
        if (_timer.Completed || _timer.Paused)
            _prevHealth = prevCurrentHealth;

        Color c = this.BarImage.color;
        c.a = 1.0f;
        this.BarImage.color = c;
        this.Bar.TargetLength = totalBarLength;
        this.Bar.UpdateLength(_prevHealth - currentHealth, maxHealth);
        this.Bar.BarTransform.anchoredPosition = new Vector2(Mathf.RoundToInt(totalBarLength * ((float)currentHealth / (float)maxHealth)), _y);
        _timer.reset();
        _timer.start();
    }

    /**
     * Private
     */
    private Timer _timer;
    private int _y;
    private int _prevHealth;

    private void playAnimation()
    {
        this.Animation.Play();
    }
}
