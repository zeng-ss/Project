//------------------------------------------------------------
// File : ConditionEventManager.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections.Generic;

public class ConditionEventManager : MonoSingleton<ConditionEventManager>
{
    public class ConditionEvent
    {
        public Action action;
        public Func<bool> condition;
    }

    public List<ConditionEvent> m_events = new List<ConditionEvent>();

    public void EnqueueAction(Action action, Func<bool> condition)
    {
        ConditionEvent _event = new ConditionEvent();
        _event.action = action;
        _event.condition = condition;
        m_events.Add(_event);
    }

    public void Update()
    {
        if (m_events.Count > 0)
        {
            for (int j = m_events.Count - 1; j >= 0; j--)
            {
                if (m_events[j].condition.Invoke())
                {
                    m_events[j].action?.Invoke();
                    m_events.RemoveAt(j);
                    return;
                }
            }
        }
    }
}