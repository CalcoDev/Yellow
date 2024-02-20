using System;

namespace Yellow.Misc;

public class StateMachine
{
    public int State
    {
        get => _state;
        set
        {
            if (_state == value || value < 0 || value >= _stateCount)
            {
                return;
            }

            _previousState = _state;
            _state = value;

            if (_previousState != -1)
            {
                _exits[_previousState]?.Invoke();
            }
            _enters[_state]?.Invoke();
        }
    }

    public StateMachine(int maxStates, int defaultState)
    {
        _stateCount = maxStates;

        _enters = new Action[maxStates];
        _updates = new Func<int>[maxStates];
        _fixedUpdates = new Action[maxStates];
        _exits = new Action[maxStates];
        _previousState = _state = defaultState;
    }

    public void Update()
    {
        int newState = _updates[_state]?.Invoke() ?? _state;
        State = newState;
    }

    public void FixedUpdate()
    {
        _fixedUpdates[_state]?.Invoke();
    }

    public void SetCallbacks(int index, Func<int> update, Action enter = null,
        Action exit = null, Action fixedUpdate = null)
    {
        _enters[index] = enter;
        _updates[index] = update;
        _fixedUpdates[index] = fixedUpdate;
        _exits[index] = exit;
    }

    private int _state;
    private int _previousState;

    private readonly int _stateCount;
    private readonly Action[] _enters;
    private readonly Func<int>[] _updates;
    private readonly Action[] _fixedUpdates;
    private readonly Action[] _exits;

}