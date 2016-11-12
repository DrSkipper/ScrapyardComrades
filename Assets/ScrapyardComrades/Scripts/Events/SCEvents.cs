﻿using UnityEngine;

public class CollisionEvent : LocalEventNotifier.Event
{
    public const string NAME = "COLLISION";
    public GameObject[] Hits;
    public Vector2 VelocityAtHit; // Velocity of actor at time collision was detected, before being multiplied by Time.deltaTime
    public Vector2 VelocityApplied; // How much of the velocity, AFTER Time.deltaTime multiplier, was applied before detecting the collision

    public CollisionEvent(GameObject[] hits, Vector2 velocity, Vector2 velocityApplied)
    {
        this.Name = NAME;
        this.Hits = hits;
        this.VelocityAtHit = velocity;
        this.VelocityApplied = velocityApplied;
    }
}

public class HitEvent : LocalEventNotifier.Event
{
    public const string NAME = "DAMAGE";
    public GameObject Hit;

    public HitEvent(GameObject hit)
    {
        this.Name = NAME;
        this.Hit = hit;
    }
}

public class CharacterUpdateFinishedEvent : LocalEventNotifier.Event
{
    public const string NAME = "UPDATE_FINISHED";
    public SCAttack CurrentAttack;

    public CharacterUpdateFinishedEvent(SCAttack currentAttack = null)
    {
        this.Name = NAME;
        this.CurrentAttack = currentAttack;
    }
}

public class GameplayPausedEvent : LocalEventNotifier.Event
{
    public const string NAME = "GAMEPLAY_PAUSE";
    public bool Paused;

    public GameplayPausedEvent(bool paused)
    {
        this.Name = NAME;
        this.Paused = paused;
    }
}

public class SequencePausedEvent : LocalEventNotifier.Event
{
    public const string NAME = "SEQUENCE_PAUSE";
    public bool Paused;

    public SequencePausedEvent(bool paused)
    {
        this.Name = NAME;
        this.Paused = paused;
    }
}

public class FreezeFrameEvent : LocalEventNotifier.Event
{
    public const string NAME = "FREEZE_FRAME";
    public int NumFrames;

    public FreezeFrameEvent(int numFrames)
    {
        this.Name = NAME;
        this.NumFrames = numFrames;
    }
}

public class FreezeFrameEndedEvent : LocalEventNotifier.Event
{
    public const string NAME = "FREEZE_FRAME_END";

    public FreezeFrameEndedEvent()
    {
        this.Name = NAME;
    }
}

public class HitStunEvent : LocalEventNotifier.Event
{
    public const string NAME = "HIT_STUN";
    public int NumFrames;

    public HitStunEvent(int numFrames)
    {
        this.Name = NAME;
        this.NumFrames = numFrames;
    }
}

public class PlayerSpawnedEvent : LocalEventNotifier.Event
{
    public const string NAME = "PLAYER_SPAWNED";
    public GameObject PlayerObject;

    public PlayerSpawnedEvent(GameObject playerObject)
    {
        this.Name = NAME;
        this.PlayerObject = playerObject;
    }
}

public class WorldRecenterEvent : LocalEventNotifier.Event
{
    public const string NAME = "WORLD_RECENTER";
    public IntegerVector RecenterOffset;

    public WorldRecenterEvent(IntegerVector recenterOffset)
    {
        this.Name = NAME;
        this.RecenterOffset = recenterOffset;
    }
}

public class SCSpriteAnimationLoopEvent : LocalEventNotifier.Event
{
    public static string NAME = "ANIM_LOOP";
    public int NewElapsed;

    public SCSpriteAnimationLoopEvent(int newElapsed)
    {
        this.Name = NAME;
        this.NewElapsed = newElapsed;
    }
}

public class PauseEvent : LocalEventNotifier.Event
{
    public static string NAME = "PAUSE";
    public PauseController.PauseGroup PauseGroup;

    public PauseEvent(PauseController.PauseGroup pauseGroup)
    {
        this.Name = NAME;
        this.PauseGroup = pauseGroup;
    }
}

public class ResumeEvent : LocalEventNotifier.Event
{
    public static string NAME = "RESUME";
    public PauseController.PauseGroup PauseGroup;

    public ResumeEvent(PauseController.PauseGroup pauseGroup)
    {
        this.Name = NAME;
        this.PauseGroup = pauseGroup;
    }
}