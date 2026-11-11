using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class YieldHepler
{

    public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

    public static IEnumerator WaitForSeconds(float totalTime, bool ignoreScaleTime = false)
    {
        float time = 0;
        while (time < totalTime)
        {
            time += ignoreScaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    public static IEnumerator WaitForFrame(int frame)
    {
        int _frame = 0;
        while (_frame < frame)
        {
            yield return null;
            _frame++;
        }
    }



    public static void CallAfterOnFrame(System.Action action)
    {
        MEC.Timing.RunCoroutine(CallAfterOnFrameIE(action));
    }
    public static void CallAfterOnSeconds(System.Action action, float times)
    {
        MEC.Timing.RunCoroutine(CallAfterOnSecondsIE(action, times));
    }

    public static void CallUntilTrue(System.Func<bool> func, System.Action action)
    {
        MEC.Timing.RunCoroutine(CallUntilTrueIE(func, action));
    }

    private static IEnumerator<float> CallAfterOnFrameIE(System.Action action)
    {
        yield return 0;
        action?.Invoke();
    }
    private static IEnumerator<float> CallAfterOnSecondsIE(System.Action action, float times)
    {
        yield return MEC.Timing.WaitForSeconds(times);
        action?.Invoke();
    }

    private static IEnumerator<float> CallUntilTrueIE(System.Func<bool> func, System.Action action)
    {
        yield return MEC.Timing.WaitUntilTrue(func);
        action?.Invoke();
    }

}