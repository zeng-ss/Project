namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// CarSort 道具基类：持有核心与对象池引用。每种道具一个脚本，继承该类并实现 Execute。
    /// </summary>
    public abstract class PropBase
    {
        protected readonly CoffeeAwayCore Core;

        protected PropBase(CoffeeAwayCore core)
        {
            Core = core;
        }

        public abstract ItemType ItemType { get; }

        // 统一执行入口：各道具按需读取 targetHolder / targetColor
        public abstract bool Execute(Holder targetHolder = null, ColorType targetColor = default);
    }
}
