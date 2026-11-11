
using UnityEngine;
using CasualGame;
using DG.Tweening;

namespace CasualGame
{
    public class LoadingDialog : UGuiForm
    {
        private Transform m_Loading => GetRef<Transform>("Loading");
        private TMPro.TextMeshProUGUI m_Text => GetRef<TMPro.TextMeshProUGUI>("Text");
        private float passTime;
        private float duration;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_Loading.rotation = Quaternion.identity;
            m_Loading.DORotate(new Vector3(0, 0, -360), 5f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);

            if (MParams.GetBoolParams("isPurchase"))
            {
                m_Text.text = LocalizationMgr.Instance.GetString("43");
            }
            else
            {
                m_Text.text = LocalizationMgr.Instance.GetString("44");
            }

            passTime = 0f;
            duration = MParams.GetFloatParams("Duration");
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            passTime += Time.deltaTime;
            if (passTime > duration)
            {
                Close();
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

        }
    }
}