using UnityEngine;
using System.Collections.Generic;

public class InvisiLight : MonoBehaviour, IPausable
{
    public float StartAngle = 0.0f;
    public float RotationSpeed = 1.0f;
    public SCCharacterController.Facing Direction;
    public Transform RotationTransform;
    public IntegerCollider[] RangeColliders;
    public LayerMask DetectionLayers;

    void OnSpawn()
    {
        if (_objectsInRange == null)
            _objectsInRange = new List<InvisibilityHandler>();
        if (_detected == null)
            _detected = new List<GameObject>();

        _angle = this.StartAngle;
        this.RotationTransform.SetRotZ(this.StartAngle);

        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    void FixedUpdate()
    {
        _angle += this.RotationSpeed * -(int)this.Direction;
        this.RotationTransform.SetRotZ(_angle);

        for (int i = 0; i < this.RangeColliders.Length; ++i)
        {
            this.RangeColliders[i].Collide(_detected, 0, 0, this.DetectionLayers);
        }

        for (int i = 0; i < _objectsInRange.Count;)
        {
            int foundIndex = _detected.IndexOf(_objectsInRange[i].gameObject);
            if (foundIndex < 0)
            {
                _objectsInRange[i].Invisible = false;
                _objectsInRange.RemoveAt(i);
            }
            else
            {
                _detected.RemoveAt(foundIndex);
                ++i;
            }
        }

        for (int i = 0; i < _detected.Count; ++i)
        {
            InvisibilityHandler handler = _detected[i].GetComponent<InvisibilityHandler>();
            if (handler != null)
            {
                handler.Invisible = true;
                _objectsInRange.Add(handler);
            }
        }

        _detected.Clear();
    }

    void OnReturnToPool()
    {
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, WorldRecenterEvent.NAME);
        if (_objectsInRange != null)
            _objectsInRange.Clear();
    }

    /**
     * Private
     */
    private float _angle;
    private List<InvisibilityHandler> _objectsInRange;
    private List<GameObject> _detected;

    private void onRecenter(LocalEventNotifier.Event e)
    {
        if (_objectsInRange != null)
        {
            for (int i = 0; i < _objectsInRange.Count; ++i)
            {
                _objectsInRange[i].Invisible = false;
            }
            _objectsInRange.Clear();
        }
    }
}
