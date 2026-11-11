/// <summary>
/// 位运算工具。替代原 GameFramework.Utility.Bitwise。
/// </summary>
public static class Utility
{
    public static class Bitwise
    {
        /// <summary>判断 value 的 mask 位是否为 1。</summary>
        public static bool IsBit(int value, int mask) => (value & mask) != 0;

        /// <summary>设置 value 的 mask 位。</summary>
        public static void SetBit(ref int value, int mask, bool on)
        {
            if (on) value |= mask;
            else value &= ~mask;
        }
    }
}
