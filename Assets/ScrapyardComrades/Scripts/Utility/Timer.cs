﻿using UnityEngine;
using System.Collections;

public class Timer
{
	public delegate void TimerCallback();
	public TimerCallback callback;
	public bool paused;
	public bool loops;
	public bool completed { get; private set; }
    public float timeRemaining { get { return _timeRemaining; } }

	public Timer(float duration, bool loops = false, bool startsImmediately = true, TimerCallback callback = null)
	{
		_timeRemaining = _duration = duration;
		this.loops = loops;
		this.callback = callback;
		this.paused = !startsImmediately;
	}

	public void start()
	{
		this.paused = false;
	}

    public void reset()
    {
        _timeRemaining = _duration;
        this.completed = false;
    }

    public void reset(float duration)
    {
        _duration = duration;
        this.reset();
    }

    public void complete()
    {
        if (this.callback != null)
            this.callback();

        if (this.loops)
            _timeRemaining = _duration;
        else
            this.completed = true;
    }

	public void update(float dt)
	{
		if (!this.paused && !this.completed)
		{
			_timeRemaining -= dt;

			if (_timeRemaining <= 0.0f)
                this.complete();
		}
	}

	public void invalidate()
	{
		this.callback = null;
		this.completed = true;
	}

	/**
	 * Private
	 */
	private float _duration;
	private float _timeRemaining;
}
