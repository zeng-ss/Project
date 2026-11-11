using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * EventManager 版本: V1.0.0，设计思路：
 * 1. 通过通知的形式，解耦合
 * 2. 只实现添加、移除、通知三种API，逻辑简单，便于理解和后续修改
 * 3. 事件传递参数使用C#原生EventArgs，特殊对象传递通过继承来进行定制，比params object[]这种通过封包解包的形式性能好，且更易理解
 */
public abstract class CustomEventArgs
{

}

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate(CustomEventArgs eventArgs = null);

    private readonly Dictionary<int, List<EventDelegate>> _eventDelegateDictionary =
        new Dictionary<int, List<EventDelegate>>();

    public static void AddListener(int eventID, EventDelegate eventDelegate)
    {
        if (!Instance._eventDelegateDictionary.TryGetValue(eventID, out List<EventDelegate> eventDelegateList))
        {
            eventDelegateList = new List<EventDelegate>();
            Instance._eventDelegateDictionary.Add(eventID, eventDelegateList);
        }

        if (eventDelegateList.Contains(eventDelegate))
        {
            Debug.LogError("Event: " + eventID + " is already added");
        }
        else
        {
            eventDelegateList.Add(eventDelegate);
        }
    }

    public static void RemoveListener(int eventID, EventDelegate eventDelegate)
    {
        if (!Instance._eventDelegateDictionary.TryGetValue(eventID, out List<EventDelegate> eventDelegateList))
        {
            Debug.LogError("Event: " + eventID + " is not added");
            return;
        }

        if (!eventDelegateList.Contains(eventDelegate))
        {
            Debug.LogError("Event: " + eventID + " is not added this eventDelegate");
            return;
        }

        eventDelegateList.Remove(eventDelegate);
    }

    public static void NotifyEvent(int eventID, CustomEventArgs eventArgs = null)
    {
        if (!Instance._eventDelegateDictionary.TryGetValue(eventID, out List<EventDelegate> eventDelegateList)) return;

        foreach (var eventDelegate in eventDelegateList)
        {
            try
            {
                eventDelegate.Invoke(eventArgs);
            }
            catch (Exception e)
            {
                Debug.LogError($"Event: {eventID} have exception, some eventDelegate error : {e.Message}\n{e.StackTrace}");
            }
        }
    }
}