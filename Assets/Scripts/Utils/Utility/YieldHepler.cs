using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public class YieldHelperKey
    {
        public const int Guide = 1;
        public const int PlotsPlay = 50;
        public const int AreaUnlock = 500;

        public const int AreaGuide = 1000;
        public const int ScanCard = 1200;
    }
    public static class YieldHepler
    {
        public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        static YieldHepler()
        {
            SortedSet = new List<int>();
        }

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

        public static void CallAfterOneFrame(System.Action action)
        {
            MEC.Timing.RunCoroutine(CallAfterOnFrameIE(action));
        }

        public static void CallAfterTwoFrames(System.Action action)
        {
            MEC.Timing.RunCoroutine(CallAfterTwoFramesIE(action));
        }

        public static void CallAfterOnSeconds(System.Action action, float times)
        {
            MEC.Timing.RunCoroutine(CallAfterOnSecondsIE(action, times));
        }

        public static void CallUntilTrue(System.Func<bool> func, System.Action action, GameObject cancelWith)
        {
            MEC.Timing.RunCoroutine(CallUntilTrueIE(func, action).CancelWith(cancelWith));
        }

        public static List<int> SortedSet = new List<int>();

        public static CoroutineHandle CallUntilTrue(System.Func<bool> func, System.Action action)
        {
            return MEC.Timing.RunCoroutine(CallUntilTrueIE(func, action));
        }

        public static void CallAfterOneFrameSingleton(System.Action action)
        {
            MEC.Timing.RunCoroutineSingleton(CallAfterOnFrameIE(action), "CallAfterOnFrameSingleton",
                MEC.SingletonBehavior.Abort);
        }

        private static IEnumerator<float> CallAfterOnFrameIE(System.Action action)
        {
            yield return 0;
            action?.Invoke();
        }

        private static IEnumerator<float> CallAfterTwoFramesIE(System.Action action)
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
}