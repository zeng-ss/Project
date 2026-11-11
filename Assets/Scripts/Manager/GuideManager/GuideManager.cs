using System;
using System.Collections;
using UnityEngine;
using GameFramework.DataTable;

namespace CasualGame
{
    public partial class GuideManager
    {
        public static IDataTable<DRGuide> DrGuideDataTable => GameEntry.DataTable.GetDataTable<DRGuide>();
        private static int _currentGuideId = -1;
        public static GuideKey CurrentGuideKey => (GuideKey)_currentGuideId;
        private static Action _finishGuideEvent;

        public static void FinishGuide()
        {
            DRGuide drGuide = DrGuideDataTable.GetDataRow(_currentGuideId);

            _currentGuideId = -1;

            bool hasNextGuide = false;
            if (drGuide != null)
            {
                if (drGuide.NextId > 0)
                {
                    hasNextGuide = true;
                    ExecuteGuide((GuideKey)drGuide.NextId);
                }
            }

            if (!hasNextGuide)
            {
                _finishGuideEvent?.Invoke();
                _finishGuideEvent = null;
            }
        }

        public static void ExecuteGuide(GuideKey guideKey, Action finishGuideEvent = null)
        {
            int guideId = (int)guideKey;

            if (guideId <= 0) return;

            if (_currentGuideId == guideId) return;

            if (_finishGuideEvent == null && finishGuideEvent != null)
            {
                _finishGuideEvent = finishGuideEvent;
            }

            //如果当前引导还在，需要主动删除
            _currentGuideId = guideId;

            DRGuide guideData = DrGuideDataTable.GetDataRow(guideId);
            if (guideData == null)
            {
                Debug.LogError($"ExecuteGuide fail ,DRGuide is null id={guideId}");
                return;
            }

            GameEntry.Base.StartCoroutine(WaitGuideFormOpen((UIFormType)guideData.WaitUIOpenType, () =>
            {
                OpenGuideUI(guideId);
            }));
        }

        private static void OpenGuideUI(int guideID)
        {
            CheckToStopGuide();

            EventSystemTool.DisableEventSystem();
            UGUIParams @params = UGUIParams.Create();
            @params.AddValue("GuideId", guideID);
            @params.OnOpenCallback = (ui) =>
            {
                EventSystemTool.EnableEventSystem();
            };

            GameEntry.UI.OpenUIForm(UIFormType.GuideUI, @params);
        }

        static IEnumerator WaitGuideFormOpen(UIFormType formType, System.Action CallBack)
        {
            if (formType < 0)
            {
                CallBack?.Invoke();
                yield break;
            }

            UGuiForm form = GameEntry.UI.GetUIForm<UGuiForm>(formType);
            if (form != null && form.UIAnimationFinish)
            {
                CallBack?.Invoke();
                yield break;
            }

            int count = 0;
            EventSystemTool.DisableEventSystem();
            while ((form == null || (form != null && !form.UIAnimationFinish)) && count < 300)
            {
                if (form == null)
                    form = GameEntry.UI.GetUIForm<UGuiForm>(formType);
                yield return new WaitForFixedUpdate();
                count++;
            }

            EventSystemTool.EnableEventSystem();

            CallBack?.Invoke();
        }

        private static void CheckToStopGuide()
        {
            var form = GameEntry.UI.GetUIForm<UGuiForm>(UIFormType.GuideUI);
            if (form != null) form.Close();
        }
    }

    public enum GuideKey
    {
        Car5 = 1,
        Car2 = 2,
        Car4 = 3,
        Car1 = 4,
        Car0 = 5,
    }
}