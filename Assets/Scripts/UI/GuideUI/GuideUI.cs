using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public enum GuideStyle
    {
        Finger = 1, //手指点击
        Dialog = 2, // 对话框
    }

    public class GuideUI : UGuiForm
    {
        // AutoBindFieldInfo
        [Header("PrefabBinderTool")]
        [SerializeField] private GuideDialogCell DialogCell_GuideDialogCell;

        [SerializeField] private UnityEngine.UI.Button Button;

        [SerializeField] private UnityEngine.UI.Image BackGround_Image;

        [SerializeField] private GuideFingerCell FingerCell_GuideFingerCell;

        // AutoBindFieldInfo

        private UnityEngine.UI.Image m_TipsFrame => GetRef<UnityEngine.UI.Image>("TipsFrame");
        private TMPro.TextMeshProUGUI m_TipsContext => GetRef<TMPro.TextMeshProUGUI>("TipsContext");

        private DRGuide _drGuide;

        private int _guideID = 0;
        private int _currentAudioID = 0;
        private string _contentText;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            Button.onClick.AddListener(OnClick_Button);
        }


        private void OnClick_Button()
        {
            //强制引导中指定类型之外不可以全屏幕点击
            if ((GuideStyle)_drGuide.GuideType == GuideStyle.Finger)
            {
                return;
            }

            //PlaySoundManager.Instance.PlaySound(Constant.Audio.click_start);
            Close();

            GuideManager.FinishGuide();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_TipsFrame.gameObject.SetActive(false);

            _guideID = MParams.GetIntParams("GuideId");
            _drGuide = GuideManager.DrGuideDataTable.GetDataRow(_guideID);

            if (_drGuide == null)
            {
                GuideManager.FinishGuide();
                Close();
                return;
            }

            ShowGuide();
        }

        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

        private void ShowGuide()
        {
            GuideStyle guideStyle = (GuideStyle)_drGuide.GuideType;

            DialogCell_GuideDialogCell.gameObject.SetActive(guideStyle == GuideStyle.Dialog);
            FingerCell_GuideFingerCell.gameObject.SetActive(guideStyle == GuideStyle.Finger);

            // if (m_GuideData.Raycase)
            // {
            //     GuideHelperTool.StartGuiding(GuideHelperToolKey.GuideUI);
            // }

            // Color backGroundColor = BackGround_Image.color;
            // backGroundColor.a = guideStyle == GuideStyle.Finger ? 0.7f : 0f;
            // BackGround_Image.color = backGroundColor;

            switch (guideStyle)
            {
                case GuideStyle.Finger:
                {
                    DRFingerGuide drFingerGuide =
                        Framework.GameRoot.Config.GetTable<DRFingerGuide>().GetDataRow(_drGuide.Id);

                    FingerCell_GuideFingerCell.Guide(drFingerGuide, Close);
                    break;
                }
                case GuideStyle.Dialog:
                {
                    DRDialogGuide drDialogGuide =
                        Framework.GameRoot.Config.GetTable<DRDialogGuide>().GetDataRow(_drGuide.Id);

                    DialogCell_GuideDialogCell.ShowDialog(drDialogGuide);
                    break;
                }
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);


            _drGuide = null;

            m_TipsFrame.gameObject.SetActive(false);
        }
    }
}