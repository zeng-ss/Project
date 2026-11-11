//------------------------------------------------------------
// File : VibrateManager.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 震动管理
//------------------------------------------------------------
using UnityEngine;
using CasualGame;
using CasualGame.CoffeeAway;

public class VibrateManager : MonoSingleton<VibrateManager>
{
    /// <summary>
    /// Unity 提供的单次震动时长
    /// </summary>
    public readonly float VibrateOnceTime = 0.015f;
    public float VibrateInterval = 0;

    public int androidAmplitude = 100;
    public int iOSAmplitude = 0;

    private bool isPlaying = false;
    private float timer = 0;  //定时器事件
    private float totalTime = 0;
    private bool isMuted => ProxyManager.UserProxy.IsMediaMuted((int)Media.shock);

    /// <summary>
    /// 循环播放震动
    /// </summary>
    public void PlayVibrateLoop()
    {
        if (isMuted) return;

        isPlaying = true;
        timer = 0;
        totalTime = float.MaxValue;
        VibrateInterval = 0.1f;
    }

    public void PlayOnce()
    {
        if (isMuted) return;

        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = 100;
        iOSAmplitude = 0;
    }

    public void PlayOnceStrength()
    {
        if (isMuted) return;

        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = 160;
        iOSAmplitude = 1;
    }

    public void PlayOnceWithStrength(int strength)
    {
        if (isMuted) return;

        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = strength;
        iOSAmplitude = strength;
    }

    public void StopVibrate()
    {
        isPlaying = false;
        timer = 0;
        totalTime = 0;
        VibrateInterval = 0;
    }

    public void PlayOnceOnly()
    {
        if (isMuted) return;
        NativeBridge.Vibrate(20, 150);
    }

    public void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (timer > .0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = VibrateOnceTime + VibrateInterval;
            totalTime -= VibrateOnceTime + VibrateInterval;
            if (totalTime < 0)
            {
                isPlaying = false;
            }
            else
            {
#if UNITY_ANDROID
                NativeBridge.Vibrate((int)(VibrateOnceTime * 1000), androidAmplitude);
#elif UNITY_IOS
                NativeBridge.Vibrate((int)(VibrateOnceTime * 1000), iOSAmplitude);
#endif
            }
        }
    }
}