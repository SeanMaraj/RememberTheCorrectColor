using UnityEngine;
using System;
using System.Collections;

public class TickingTimer 
{
	enum State { Initialized, Ticking, Disposed };
	State _state;
	Action _stateEnder;

	float _tickDuration;
	int _repeats;
	Action _tickAction;
	MonoBehaviour _context;
	
	int _repeatsCounter = 0;
	bool _timerActive;


	public TickingTimer (float tickDuration, int repeats, Action tickAction, MonoBehaviour context)
	{
		_tickDuration = tickDuration;
		_repeats = repeats;
		_tickAction = tickAction;
		_context = context;

		toInitialized();
	}

	void enterState(State state, Action stateEnder)
	{
		if (_stateEnder != null)
		{
			_stateEnder();
		}

		_state = state;
		_stateEnder = stateEnder;
	}

	/*
	 * EVENTS
	*/
	void runTimer()
	{
		if (_timerActive && _state == State.Ticking)
		{
			_repeatsCounter++;
			_tickAction();

			if ((_repeats > 0) && (_repeatsCounter >= _repeats))
			{
				toDisposed();
			}else
			{
				DelayedCallback(runTimer, _tickDuration);
			}
		}
	}

	public void destroy()
	{
		toDisposed();
	}

	/*
	 * STATE METHODS
	*/
	void toInitialized()
	{
		enterState(State.Initialized, endInitialized);
		toTicking();
	}

	void endInitialized()
	{

	}

	void toTicking()
	{
		enterState(State.Ticking, endTicking);
		_timerActive = true;
		DelayedCallback(runTimer, _tickDuration);
	}

	void endTicking()
	{
		_timerActive = false;
	}

	void toDisposed()
	{
		enterState(State.Disposed, endDisposed);
	}

	void endDisposed()
	{

	}

	/*
	 * HELPER METHODS
	*/
	//TODO: fix
	public void DelayedCallback( Action callback, float delay)
	{ 
		_context.StartCoroutine( Delay ( callback, delay));
	}
	
	IEnumerator Delay( Action callback, float delay)
	{ 
		yield return new WaitForSeconds(delay);
		callback(); 
	}

	public void setDuration(float duration)
	{
		_tickDuration = duration;
	}

}