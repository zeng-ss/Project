using UnityEngine;
using System.Collections.Generic;
using Custom.Activity;
using Cysharp.Threading.Tasks;
using System;

namespace CasualGame
{
    public enum BackHomeType
    {
        None,
        ColdBoot, //冷启动回到主页
        WordScrew, //从Word 界面回来
    }

    public class HomeTaskManager : Singleton<HomeTaskManager>
    {
        private readonly LinkedList<IHomeTask> m_HomeTasks;
        private UIMainForm m_HomeForm;

        private bool isColdStart = true;
        private bool needCheckNewFunc;

        public HomeTaskManager()
        {
            m_HomeTasks = new LinkedList<IHomeTask>();
        }

        public void Init(UIMainForm homeForm)
        {
            if (isColdStart)
            {
                isColdStart = false;
            }
            this.m_HomeForm = homeForm;

            // 打开引导
            // if (m_HomeTasks.Count > 0)
            // {
            //     KBTutorialManager.Instance.tutorialForm.SetMasking(true);
            // }
        }

        public void Clear()
        {
            m_HomeTasks.Clear();
            m_HomeForm = null;
        }

        //添加任务
        public void AddTaskLast(IHomeTask task)
        {
            m_HomeTasks.AddLast(task);
        }

        public void AddTaskFirst(IHomeTask task)
        {
            m_HomeTasks.AddFirst(task);
        }


        //退出主界面之后就不执行了
        public void Update()
        {
            if (m_HomeForm == null) return;

            LinkedListNode<IHomeTask> current = m_HomeTasks.First;
            if (current != null)
            {
                current.Value.DoExecute(m_HomeForm);
                if (!current.Value.Done)
                {
                    current.Value.Update();
                }
                else
                {
                    m_HomeTasks.Remove(current);
                    if (m_HomeTasks.Count <= 0)
                    {
                        //关闭引导MASK
                        // KBTutorialManager.Instance.tutorialForm.SetMasking(false);
                    }
                }
            }
            else if (needCheckNewFunc)
            {
                ActivityManager.Instance.CheckNewFunction();
                needCheckNewFunc = false;
            }
        }



        //         public bool DoneAll => m_HomeTasks.Count <= 0;

        public bool CheckNewFunction()
        {
            return ActivityManager.Instance.CheckNewFunction();
        }

        public async void CheckAutoPopup(BackHomeType backHome)
        {
            ActivityManager.Instance.CheckAutoPopup(backHome);
        }


        //         private void ProcessResumePurchase(BackHomeType backHome)
        //         {
        //             if (backHome == BackHomeType.ColdBoot)
        //             {
        //                 if (GUIAPManager.Instance.PurchasedProducts.Count <= 0)
        //                     return;
        //                 int totalCount = 0; //最多处理20个购买恢复任务，避免卡死
        //                 while (GUIAPManager.Instance.PurchasedProducts.Count > 0 && totalCount < 20)
        //                 {
        //                     string product = GUIAPManager.Instance.PurchasedProducts.Dequeue();
        //                     Action action = GUIAPManager.Instance.GetPurchasedProduct(product, false);
        //                     HomeAction homeAction = new HomeAction(action, () => !HasPopForm());
        //                     AddTaskLast(homeAction);
        //                     totalCount++;
        //                 }
        //             }
        //         }

        //         private bool HasPopForm()
        //         {
        //             int popup = Framework.UIMgr.Instance.CurrentPopUpCount() + Framework.UIMgr.Instance.CurrentDefaultUpCount();
        //             return popup >= 2;
        //         }
    }
}
