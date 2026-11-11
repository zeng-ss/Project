using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoSingleton<CoroutineHelper>
{
    public void DoActionDelay(float delay, System.Action action)
    {
        StartCoroutine(ActionDelay(delay, action));
    }

    IEnumerator ActionDelay(float delay, System.Action action)
    {
        yield return YieldHepler.WaitForSeconds(delay);
        action?.Invoke();
    }
}
