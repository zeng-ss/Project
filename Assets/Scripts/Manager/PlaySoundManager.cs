using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 音频播放管理器：按 soundId 查配置表，走 ResMgr 加载音频，交给 SoundManager 播放。
    /// 同个 soundId 在 0.1s 内只播放一次。
    /// </summary>
    public class PlaySoundManager : Singleton<PlaySoundManager>
    {
        private const float DUPLICATE_PLAY_INTERVAL = 0.1f;

        private readonly Dictionary<int, float> _lastPlayTimeDict = new Dictionary<int, float>();

        public void Init()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        }

        /// <summary>切换设备（蓝牙）后重新播放背景音乐。</summary>
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

        /// <summary>播放音效。</summary>
        public int? PlaySound(int soundId)
        {
            if (IsInDuplicateInterval(soundId)) return null;

            _lastPlayTimeDict[soundId] = Time.realtimeSinceStartup;
            Play(soundId, false);
            return soundId;
        }

        /// <summary>播放背景音乐。</summary>
        public void PlayBgm()
        {
            Play(SceneHelper.IsGamePlayScene() ? Constant.Audio.bgm_game : Constant.Audio.bgm_home, true);
        }

        public void ResumeAudio()
        {
            AudioListener.pause = false;
            PlayBgm();
        }

        public void StopAudio()
        {
            AudioListener.pause = true;
        }

        private static void Play(int soundId, bool isBgm)
        {
            DRSound drSound = GameRoot.Config?.GetTable<DRSound>()?.GetDataRow(soundId);
            if (drSound == null)
            {
                Debug.LogWarning($"[PlaySoundManager] 找不到音效配置: {soundId}");
                return;
            }

            string path = ResourceIdentificationTool.Instance.GetAssetPathById(drSound.AssetId);
            if (string.IsNullOrEmpty(path)) return;

            GameRoot.Res.LoadAssetAsync<AudioClip>(path, clip =>
            {
                if (clip == null) return;

                if (isBgm) GameRoot.Sound.PlayBgm(clip, drSound.Loop, drSound.Volume);
                else GameRoot.Sound.PlaySfx(clip, drSound.Volume);
            });
        }

        private bool IsInDuplicateInterval(int soundId)
        {
            if (!_lastPlayTimeDict.TryGetValue(soundId, out float lastPlayTime)) return false;

            return Time.realtimeSinceStartup - lastPlayTime < DUPLICATE_PLAY_INTERVAL;
        }
    }
}
