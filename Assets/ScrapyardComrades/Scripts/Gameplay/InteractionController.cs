using UnityEngine;
using System.Collections.Generic;

public class InteractionController : VoBehavior
{
    public SCCharacterController CharacterController;
    public IntegerCollider InteractionBox;
    public LayerMask InteractionMask;

    void Awake()
    {
        _collisions = new List<GameObject>();
        _nearbyColliders = new List<IntegerCollider>();
        _targetChangeEvent = new InteractionTargetChangeEvent(null);
    }

    void Start()
    {
        this.localNotifier.Listen(CharacterUpdateFinishedEvent.NAME, this, postUpdate);
    }

    void OnSpawn()
    {
        gatherNearbyColliders();
        _framesUntilColliderGet = FRAME_OFFSET % FRAMES_BETWEEN_COLLIDER_GET;
        FRAME_OFFSET = FRAME_OFFSET >= 10000 ? 0 : FRAME_OFFSET + 1;
    }

    /**
     * Private
     */
    private GameObject _highlightedObject;
    private List<GameObject> _collisions;
    private InteractionTargetChangeEvent _targetChangeEvent;
    private List<IntegerCollider> _nearbyColliders;
    private int _framesUntilColliderGet;

    private static int FRAME_OFFSET = 0;
    private const int ENLARGE_AMT = 48;
    private const int FRAMES_BETWEEN_COLLIDER_GET = 8;

    private void postUpdate(LocalEventNotifier.Event e)
    {
        --_framesUntilColliderGet;
        if (_framesUntilColliderGet < 0)
            gatherNearbyColliders();

        // Find closest interactable within range
        this.InteractionBox.Collide(_collisions, 0, 0, this.InteractionMask, null, _nearbyColliders);
        GameObject nextHighlight = null;
        float dist = float.MaxValue;
        for (int i = 0; i < _collisions.Count; ++i)
        {
            float d = Vector2.Distance(this.transform.position, _collisions[i].transform.position);
            if (d < dist)
            {
                nextHighlight = _collisions[i];
                dist = d;
            }
        }
        _collisions.Clear();

        // Switch highlighted objects
        //TODO: Animate in UI for new one, animate out UI for old one
        if (nextHighlight != _highlightedObject)
        {
            _highlightedObject = nextHighlight;
            _targetChangeEvent.Target = _highlightedObject;
            GlobalEvents.Notifier.SendEvent(_targetChangeEvent);
        }

        // If interacting, trigger interataction with object
        if (_highlightedObject != null && (e as CharacterUpdateFinishedEvent).CurrentAttack == null && this.CharacterController.MostRecentInput.Interact)
        {
            if (_highlightedObject.GetComponent<Interactable>().Interact(this))
                _framesUntilColliderGet = 0;
        }
    }

    private void gatherNearbyColliders()
    {
        this.InteractionBox.GetPotentialCollisions(0, 0, 0, 0, this.InteractionMask, _nearbyColliders, ENLARGE_AMT, ENLARGE_AMT);
        _framesUntilColliderGet = FRAMES_BETWEEN_COLLIDER_GET;
    }
}
