//------------------------------------------------------------
// File : UserManager.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using Cysharp.Threading.Tasks;
using Custom.Activity;
using System.Collections.Generic;

namespace CasualGame
{
    public partial class WebManager : Singleton<WebManager>
    {
        public static string Token
        {
            get => GameEntry.Setting.GetString("WebToken", "");
            set => GameEntry.Setting.SetString("WebToken", value);
        }

        private WebRequestManager webRequestManager = new WebRequestManager();

        private void SetAuthorization()
        {
            webRequestManager.AddHeader("Authorization", Token);
        }

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns></returns>
        public async UniTask<ServerTimeData> GetServerTime()
        {
            const string api_server_time = "/universal/server/time"; // 获取utc0

            ServerTimeData data = await webRequestManager.Get<ServerTimeData>(api_server_time);
            return data;
        }

        /// <summary>
        /// 激活
        /// </summary>
        /// <returns></returns>
        public async void Activate()
        {
            // 防止偶发网络请求失败无请求头问题
            if (!string.IsNullOrEmpty(Token))
                SetAuthorization();

            const string api_activate = "/activation"; // 激活
            UserData req = new UserData()
            {
                deviceId = UserProxy.GetDeviceId(),
                taId = UserProxy.GetDistinctId(),
                deviceModel = SystemInfo.deviceModel,
                deviceName = SystemInfo.deviceName,
                deviceType = SystemInfo.deviceType.ToString(),
                deviceSystem = SystemInfo.operatingSystem,
                deviceMemoryInMB = SystemInfo.systemMemorySize,
                pkgName = Application.identifier,
                currentVersion = Application.version.Replace(".", ""),
                channel = UserProxy.GetChannel(),
                lang = GameEntry.Localization.Language.ToString(),
                country = ProxyManager.UserProxy.userModel.countryCode
            };

            ActiveModel<UserData> data = await webRequestManager.Put<ActiveModel<UserData>>(api_activate, req, retryCount: 5);
            if (data != null)
            {
                Token = data.token;
                UserProxy.UserName = data.user.nickname;
                SetAuthorization();
                Patch();
            }
        }

        /// <summary>
        /// 上报用户数据
        /// </summary>
        public async void Patch()
        {
        }
    }
}