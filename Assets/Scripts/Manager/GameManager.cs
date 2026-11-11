//------------------------------------------------------------
// File : GameManager.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using CasualGame.CoffeeAway;

namespace CasualGame
{
    public partial class GameManager : MonoSingleton<GameManager>
    {
        public long pastTimeMillTime;
        private bool _isInit = false;
        private float _timeTick = 0f;

        private void Awake()
        {
            gameObject.GetOrAddComponent<ExceptionHelper>();
        }

        private void Update()
        {
            if (!_isInit || pastTimeMillTime <= 0) return;

            pastTimeMillTime += (int)(Time.unscaledDeltaTime * 1000);

            if (_timeTick > 1f)
            {
                _timeTick = 0f;
                EnergyManager.UpdateEnergyTimeStamp();
            }
            else
            {
                _timeTick += Time.deltaTime;
            }
        }

        private void InitSoundSettings()
        {
            GameEntry.Sound.Mute("Music", ProxyManager.UserProxy.IsMediaMuted((int)Media.music));
            GameEntry.Sound.Mute("Sound", ProxyManager.UserProxy.IsMediaMuted((int)Media.voice));
        }

        public void StartGame()
        {
            _isInit = true;
            InitSoundSettings();
        }

        public void SetServerTime(long timeStamp)
        {
            pastTimeMillTime = timeStamp > 0 ? timeStamp : TimeUtils.GetTimeStampOfMilliseconds();
        }

        public int GetTimeStampOfSeconds()
        {
            return (int)(pastTimeMillTime / 1000);
        }
    }
}