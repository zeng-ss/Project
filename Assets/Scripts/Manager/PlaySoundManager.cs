using System.Collections.Generic;
using UnityEngine;
using CasualGame;

namespace CasualGame
{
    /// <summary>
    /// 音频播放管理器
    /// 功能：同个soundId，0.1s内只能同时播放一个
    /// </summary>
    public class PlaySoundManager : Singleton<PlaySoundManager>
    {
        /// <summary>
        /// 防重复播放的时间间隔（秒）
        /// </summary>
        private const float DUPLICATE_PLAY_INTERVAL = 0.1f;

        /// <summary>
        /// 记录每个soundId的最后播放时间
        /// </summary>
        private readonly Dictionary<int, float> _lastPlayTimeDict = new Dictionary<int, float>();

        public void Init()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        }

        /// <summary>
        /// 切换设备（蓝牙）后重新播放背景音乐
        /// </summary>
        /// <param name="deviceWasChanged"></param>
        public void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            if (deviceWasChanged)
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.dspBufferSize = 64;
                AudioSettings.Reset(config);
            }

            PlayBgm();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundId">音效ID</param>
        /// <returns>播放成功返回音效序列号，失败返回null</returns>
        public int? PlaySound(int soundId)
        {
            // 检查是否在防重复播放时间间隔内
            if (IsInDuplicateInterval(soundId))
            {
                return null;
            }

            // 记录当前播放时间
            _lastPlayTimeDict[soundId] = Time.realtimeSinceStartup;

            // 调用音频扩展方法播放音效
            return GameEntry.Sound.PlaySound(soundId);
        }

        public void PlayBgm()
        {
            int soundId = Constant.Audio.bgm_home;
            if (SceneHelper.IsGamePlayScene())
            {
                soundId = Constant.Audio.bgm_game;
            }
            GameEntry.Sound.PlayMusic(soundId);
        }

        public void ResumeAudio()
        {
            AudioListener.pause = false;
            PlaySoundManager.Instance.PlayBgm();
        }

        public void StopAudio()
        {
            AudioListener.pause = true;
        }

        /// <summary>
        /// 检查指定soundId是否在防重复播放时间间隔内
        /// </summary>
        /// <param name="soundId">音效ID</param>
        /// <returns>true表示在间隔内，不应播放；false表示可以播放</returns>
        private bool IsInDuplicateInterval(int soundId)
        {
            if (!_lastPlayTimeDict.TryGetValue(soundId, out float lastPlayTime))
            {
                // 没有播放记录，可以播放
                return false;
            }

            float currentTime = Time.realtimeSinceStartup;
            float timeDifference = currentTime - lastPlayTime;

            return timeDifference < DUPLICATE_PLAY_INTERVAL;
        }

    }
}