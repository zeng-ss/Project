//------------------------------------------------------------
// File : Utility.Bitwise.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 位操作相关
//------------------------------------------------------------

namespace GameFramework
{
    public static partial class Utility
    {
        public static partial class Bitwise
        {
            /// <summary>
            /// 检查标识位
            /// </summary>
            /// <param name="state"></param>
            /// <param name="flag"></param>
            /// <returns></returns>
            public static bool IsBit(int state, int flag)
            {
                return (state & flag) != 0;
            }
            /// <summary>
            /// 设置标识位
            /// </summary>
            /// <param name="state"></param>
            /// <param name="flag"></param>
            /// <param name="value"></param>
            public static void SetBit(ref int state, int flag, bool value)
            {
                if (value)
                {
                    state |= flag;
                }
                else
                {
                    state &= ~flag;
                }
            }

            public static bool IsByteBit(byte state, int flag)
            {
                return (state & flag) != 0;
            }

            public static void SetByteBit(ref byte state, int flag, bool value)
            {
                if (value)
                {
                    state = (byte)(state | flag);
                }
                else
                {
                    state = (byte)(state & (~flag));
                }
            }
        }
    }
}