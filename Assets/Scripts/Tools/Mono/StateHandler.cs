using UnityEngine;
public abstract class StateHandler : MonoBehaviour
{
    public int group;

    private int _currentState;

    public int CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            _currentState = value;
            SetState(value);
        }
    }

    public abstract void SetState(int status);
}