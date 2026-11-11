//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFramework;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// System.Object 变量类。
    /// </summary>
    public sealed class VarAction<T> : Variable<Action<T>>
    {
        public VarAction()
        {
        }
        
        public static implicit operator Action<T>(VarAction<T> value)
        {
            return value.Value;
        }
    }
}
