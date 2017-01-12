using UnityEngine;
using System.Collections.Generic;

public class InteractionController : VoBehavior
{
    public SCCharacterController CharacterController;
    public IntegerCollider InteractionBox;
    public LayerMask InteractionMask;

    void Start()
    {
        _collisions = new List<GameObject>();
        _targetChangeEvent = new InteractionTargetChangeEvent(null);
        this.localNotifier.Listen(CharacterUpdateFinishedEvent.NAME, this, postUpdate);
    }

    /**
     * Private
     */
    private GameObject _highlightedObject;
    private List<GameObject> _collisions;
    private InteractionTargetChangeEvent _targetChangeEvent;

    private void postUpdate(LocalEventNotifier.Event e)
    {
        // Find closest interactable within range
        this.InteractionBox.Collide(_collisions, 0, 0, this.InteractionMask);
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
            _highlightedObject.GetComponent<Interactable>().Interact(this);
        }
    }
}
