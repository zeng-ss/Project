namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 道路扩容道具：道路可容纳车辆上限 +2，同步缩短最小跟车距离。
    /// </summary>
    public class ExpandRoadProp : PropBase
    {
        public override ItemType ItemType => ItemType.ExpandRoad;

        public ExpandRoadProp(CoffeeAwayCore core) : base(core) { }

        public override bool Execute(Holder targetHolder = null, ColorType targetColor = default)
        {
            Core.AddCapacity(2);
            //Log.Info("[PowerUp:ExpandRoad] 道路扩容成功");
            return true;
        }
    }
}
