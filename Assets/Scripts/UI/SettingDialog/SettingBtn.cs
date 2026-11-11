using UnityEngine;
using UnityEngine.UI;

namespace CasualGame.CoffeeAway
{
    public enum Media
    {
        music,
        voice,
        shock,
    }
    public class SettingBtn : AttachPoolPrefabBase
    {
        [SerializeField] private UnityEngine.UI.Image m_Icon;
        private UIGray uIGray;
        private AtlasLoader _atlasLoader;
        private Media media;
        private bool isMuted;

        public void SetData(Media media, AtlasLoader atlasLoader)
        {
            uIGray = GetComponent<UIGray>();
            this.media = media;
            this._atlasLoader = atlasLoader;
            this.isMuted = ProxyManager.UserProxy.IsMediaMuted((int)media);
            Button btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
            Refresh();
        }

        public void OnClick()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            LogHandler.Info($"OnClick_DotBtn: {media}", LogColor.Green);
            ProxyManager.UserProxy.SetMediaMuted((int)media, !isMuted);
            isMuted = !isMuted;
            Refresh();
        }

        private void Refresh()
        {
            switch (media)
            {
                case Media.music:
                    uIGray.IsGray = isMuted;
                    GameEntry.Sound.Mute("Music", isMuted);
                    _atlasLoader.LoadSprite(m_Icon, Constant.AtlasId.Setting, "icon_music_on");
                    break;
                case Media.voice:
                    uIGray.IsGray = isMuted;
                    GameEntry.Sound.Mute("Sound", isMuted);
                    _atlasLoader.LoadSprite(m_Icon, Constant.AtlasId.Setting, "icon_voice_on");
                    break;
                case Media.shock:
                    uIGray.IsGray = isMuted;
                    _atlasLoader.LoadSprite(m_Icon, Constant.AtlasId.Setting, "icon_shock_on");
                    break;
                    // case Media.night:
                    //     uIGray.IsGray = false;
                    //     _atlasLoader.LoadSprite(m_Icon, Constant.AtlasId.Setting, isMuted ? "icon_Day" : $"icon_Night");
                    //     break;
            }
        }
    }
}