using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 音频管理：BGM 与音效播放。音频源自动挂在传入的根节点下。
    /// </summary>
    public class SoundManager
    {
        private readonly AudioSource _bgm;
        private readonly AudioSource _sfx;

        public float BgmVolume
        {
            get => _bgm.volume;
            set => _bgm.volume = Mathf.Clamp01(value);
        }

        public float SfxVolume
        {
            get => _sfx.volume;
            set => _sfx.volume = Mathf.Clamp01(value);
        }

        public SoundManager(Transform parent)
        {
            var root = new GameObject("Sound");
            root.transform.SetParent(parent, false);

            _bgm = root.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;

            var sfxObj = new GameObject("Sfx");
            sfxObj.transform.SetParent(root.transform, false);
            _sfx = sfxObj.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
        }

        public void PlayBgm(AudioClip clip, bool loop = true, float volume = 1f)
        {
            if (clip == null) return;
            _bgm.clip = clip;
            _bgm.loop = loop;
            _bgm.volume = Mathf.Clamp01(volume);
            _bgm.Play();
        }

        public void StopBgm() => _bgm.Stop();

        /// <summary>按声音组名设置音量："Music" 为背景音乐，其余为音效。</summary>
        public void SetVolume(string soundGroupName, float volume)
        {
            if (soundGroupName == "Music") BgmVolume = volume;
            else SfxVolume = volume;
        }

        public float GetVolume(string soundGroupName)
        {
            return soundGroupName == "Music" ? BgmVolume : SfxVolume;
        }

        /// <summary>静音 / 取消静音。</summary>
        public void Mute(string soundGroupName, bool mute) => SetVolume(soundGroupName, mute ? 0f : 1f);

        public bool IsMuted(string soundGroupName) => GetVolume(soundGroupName) <= 0f;

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            _sfx.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }
}
