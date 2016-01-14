using UnityEngine;

public class SCPhysics : VoBehavior
{
    public static float TargetFPS = 60.0f;

    /*void Awake()
    {
        _physics = this;
    }*/

    void LateUpdate()
    {
        _hasRegisteredThisFrame = false;
    }

    // Access during Update
    public static float DeltaFrames
    {
        get
        {
            if (!_hasRegisteredThisFrame)
            {
                _deltaFrames = Time.deltaTime * TargetFPS;
                _previousDeltaFrames = _deltaFrames;
                _hasRegisteredThisFrame = true;
            }
            return _deltaFrames;
        }
    }

    // Access during LateUpdate
    public static float PreviousDeltaFrames
    {
        get
        {
            return _previousDeltaFrames;
        }
    }

    /**
     * Private
     */
    //private static SCPhysics _physics;
    private static bool _hasRegisteredThisFrame = false;
    private static float _deltaFrames = 1;
    private static float _previousDeltaFrames = 0;
}
