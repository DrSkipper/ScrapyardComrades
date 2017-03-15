using UnityEngine;
using UnityEngine.UI;

public class CyclePanel : VoBehavior
{
    public Image LeftArrow;
    public Image RightArrow;
    public Text Text;
    public Color ArrowRestColor;
    public Color ArrowUseColor;
    public int ArrowColorTime = 10;

    void Awake()
    {
        _leftArrowColorTimer = new Timer(this.ArrowColorTime, false, false, leftArrowDone);
        _rightArrowColorTimer = new Timer(this.ArrowColorTime, false, false, rightArrowDone);
    }

    void Update()
    {
        _leftArrowColorTimer.update();
        _rightArrowColorTimer.update();
    }

    public void CycleLeft()
    {
        this.LeftArrow.color = this.ArrowUseColor;
        _leftArrowColorTimer.reset();
        _leftArrowColorTimer.start();
    }

    public void CycleRight()
    {
        this.RightArrow.color = this.ArrowUseColor;
        _rightArrowColorTimer.reset();
        _rightArrowColorTimer.start();
    }

    /**
     * Private
     */
    private Timer _leftArrowColorTimer;
    private Timer _rightArrowColorTimer;

    private void leftArrowDone()
    {
        this.LeftArrow.color = this.ArrowRestColor;
    }

    private void rightArrowDone()
    {
        this.RightArrow.color = this.ArrowRestColor;
    }
}
