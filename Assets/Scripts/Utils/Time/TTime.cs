//------------------------------------------------------------
// File : TTime.cs
// Desc : 全局时间管理
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public static class TTime
    {
        private static bool pause = false;

        /// <summary>
        /// 暂停时间
        /// </summary>
        public static bool PasueTime
        {
            get
            {
                return pause;
            }
            set
            {
                pause = value;
            }
        }


        /// <summary>
        /// 时间缩放参数；
        /// 会整体影响Time.deltaTime的传入
        /// 值越大，游戏越快，Time.deltaTime越大；
        /// 值越小，游戏越慢，Time.deltaTime越小；
        /// </summary>
        public static float timeScale
        {
            get
            {
                return Time.timeScale;
            }
            set
            {
                Time.timeScale = value;
            }
        }

        /// <summary>
        /// 跟realtimeSinceStartup类似，唯一区别是unscaledTime在每帧里不变，而realtimeSinceStartup实时变化
        /// </summary>
        public static float unscaledTime
        {
            get
            {
                return Time.unscaledTime;
            }
        }


        /// <summary>
        /// 从游戏运行开始，经过的时间，不受timeScale影响；也不受修改客户端时间影响；也不受暂停游戏影响（暂停游戏realtimeSinceStartup继续计算）
        /// </summary>
        public static float realtimeSinceStartup
        {
            get
            {
                return Time.realtimeSinceStartup;
            }
        }


        /// <summary>
        /// 恢复时间缩放
        /// </summary>
        public static void ResumeTimeScale()
        {
            timeScale = 1;
        }



    }
}


