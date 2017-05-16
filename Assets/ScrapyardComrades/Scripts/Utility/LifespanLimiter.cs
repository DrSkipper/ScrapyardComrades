
public class LifespanLimiter : VoBehavior, IPausable
{
    public int Lifetime;

    void OnSpawn()
    {
        if (_timer == null)
            _timer = new Timer(this.Lifetime, false, true, done);
        else
        {
            _timer.reset();
            _timer.start();
        }
    }

    void FixedUpdate()
    {
        _timer.update();
    }

    /**
     * Private
     */
    private Timer _timer;

    private void done()
    {
        ObjectPools.Release(this.gameObject);
    }
}
