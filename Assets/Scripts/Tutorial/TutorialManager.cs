using CasualGame;
using NodeCanvas.Framework;
using UnityEngine;


namespace Tutorial
{
    public enum HollowType
    {
        Rect, //矩形
        Circle,//圆形
        Clone,//克隆方式
    }

    /// <summary>
    /// 与UITutorial  交互类型
    /// </summary>
    public enum ActionType
    {
        Clean, //清理当前引导
        ShowMask, //显示遮罩
        ShowTutorialTip, //显示教程提示
    }

    public interface IActionData
    {
        ActionType ActionType { get; }
    }

    public class TutorialManager : Singleton<TutorialManager>
    {
        public GameplayTutorialForm tutorialForm { get; private set; }

        private Blackboard globalBlackboard;

        private bool m_Initial = false;

        public void InitTutorial()
        {

            GameEntry.UI.OpenUIForm(UIFormType.GameplayTutorialForm, UGUIParams.Create(null, (UGuiForm ui) =>
            {
                tutorialForm = (GameplayTutorialForm)ui;
                globalBlackboard = tutorialForm.blackboard;
                globalBlackboard.AddVariable<int>(BlackboardKey.Int_ActivityID, -1);
                globalBlackboard.AddVariable<int>(BlackboardKey.Int_ActivityStep, -1);
                m_Initial = true;
            }));
        }

        public void SetVariableValue(string key, object value) { globalBlackboard.SetVariableValue(key, value); }
        public T GetVariableValue<T>(string name) { return globalBlackboard.GetVariableValue<T>(name); }


        /// <summary>
        /// 记录弱引导步骤的标记
        /// </summary>
        /// <value></value>
        public eGameBitwise BitwiseStatus
        {
            get
            {
                return (eGameBitwise)ProxyManager.UserProxy.GameBitwiseStatus;
            }
            set
            {
                ProxyManager.UserProxy.GameBitwiseStatus = (int)value;
            }
        }
    }
}
