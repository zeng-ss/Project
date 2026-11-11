namespace CasualGame
{
    /// <summary>
    /// 物品编号。
    /// </summary>
    public enum ItemType
    {
        None = 0,
        Energy = 1,
        Coin = 2,
        Undo = 3, // CarSort 撤销道具
        Shuffle = 4, // CarSort 随机打乱道具
        ExpandRoad = 5, // CarSort 道路扩容道具
        EliminateColor = 6, // CarSort 消除颜色道具

        //特殊玩法
        Hidden = 20, //问号
        Glass = 21, //玻璃
        Container = 22, //集装箱
        Colored = 23, //颜色托盘
    }
}