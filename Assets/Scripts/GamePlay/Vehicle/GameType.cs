namespace CasualGame.CoffeeAway
{
    public enum ColorType
    {
        None = -1,
        Black = 0,
        Blue = 1,
        Brown = 2,
        SkyBlue = 18,
        Cherry = 15,
        CaramelBrown = 16,
        Cyan = 3,
        DarkBlue = 17,
        DarkGreen = 5,
        Green = 4,
        Lilac = 13,
        Orange = 7,
        Pink = 8,
        Purple = 9,
        Red = 10,
        Magenta = 12,
        Yellow = 14
    }

    public enum HolderPrefabType
    {
        Standard = 0,
        Tunnel = 1,
        Extra = 2,
        Expandable = 3,
        OpenTunnel = 4,
        Colored = 5 // 颜色容器：ColorHolderPool 按颜色索引生成专用预制体，门禁只接收指定颜色
    }

    // 可移动物体类型
    public enum VehicleType
    {
        None = -1,
        Normal = 0,
        Hidden = 1, // ？车
    }

    // 容器特殊玩法类型：对应关卡 JSON 里 HolderData.states 数组每个元素的 type 字段
    public enum HolderSpecialType
    {
        None = -1,
        Glass = 2, // 玻璃
        Container = 3, // 集装箱
        Colored = 4 // 颜色托盘
    }
}