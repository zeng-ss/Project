using GameFramework.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace CasualGame
{
    public class ProcedureLaunch : ProcedureBase
    {
        private bool _initResourcesSuccess = false;

        private readonly List<Language> _supportLanguage = new List<Language>
        {
            Language.English,
            Language.Japanese,
            Language.Korean,
            Language.Russian,
            Language.French,
            Language.German,
            Language.Spanish,
            Language.Hindi,
        };

        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Application.targetFrameRate = 60;

            // 语言配置：设置当前使用的语言，如果不设置，则默认使用操作系统语言。
            InitLanguageSettings();

            // 声音配置：根据用户配置数据，设置即将使用的声音选项。
            InitSoundSettings();

            // 字体默认配置
            TMPHelper.LoadSettingsDefault();

            // 打开加载页
            LaunchPage.ChangeLaunchState(LaunchStatus.Launch);

            // 初始化资源
            GameEntry.Resource.InitResources(() => { OnInitResourcesComplete(); });
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_initResourcesSuccess)
            {
                ChangeState<ProcedurePreload>(procedureOwner);
            }
        }

        private void InitLanguageSettings()
        {
            if (GameEntry.Base.EditorResourceMode && GameEntry.Base.EditorLanguage != Language.Unspecified)
            {
                // 编辑器资源模式直接使用 Inspector 上设置的语言
                return;
            }

            Language language = GameEntry.Localization.Language;

            bool hasSetting = false;
            if (GameEntry.Setting.HasSetting(Constant.Setting.Language))
            {
                string languageString = GameEntry.Setting.GetString(Constant.Setting.Language);
                language = (Language)Enum.Parse(typeof(Language), languageString);

                hasSetting = true;
            }

            // 若是暂不支持的语言，则使用英语
            if (!_supportLanguage.Contains(language))
            {
                language = Language.English;
            }

            if (!hasSetting)
            {
                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
            }

            GameEntry.Localization.Language = language;
        }

        private void InitSoundSettings()
        {
            GameEntry.Sound.SetVolume("Music", 1.0f);
            GameEntry.Sound.SetVolume("Sound", 1.0f);
        }

        private void OnInitResourcesComplete()
        {
            _initResourcesSuccess = true;
        }
    }
}