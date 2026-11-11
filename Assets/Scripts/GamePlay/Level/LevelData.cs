using System.Collections.Generic;
using Newtonsoft.Json;

namespace CasualGame.CoffeeAway
{
    #region 关卡数据

    public class LevelData
    {
        private const string LevelAssetPathFormat = "Assets/Game/Level/Level-{0}.json";

        [JsonProperty("updateDate")] public string UpdateDate;
        [JsonProperty("isChangeCapacity")] public bool IsChangeCapacity;
        [JsonProperty("capacity")] public int Capacity;
        [JsonProperty("capacityTextPosition")] public FloatPoint CapacityTextPosition;
        [JsonProperty("gridSizeX")] public int GridSizeX;
        [JsonProperty("gridSizeY")] public int GridSizeY;
        [JsonProperty("IsCloseConveyor")] public bool? IsCloseConveyor;
        [JsonProperty("gridData")] public List<GridCellData> GridData;
        [JsonProperty("holderDataList")] public List<HolderData> HolderDataList;
        [JsonProperty("VisibleSizeX")] public int VisibleSizeX;
        [JsonProperty("VisibleSizeY")] public int VisibleSizeY;

        public static string GetLevelAssetPath(int level) => string.Format(LevelAssetPathFormat, level);

        public static LevelData FromJson(string json) => LevelParser.Parse(json);
    }

    public class FloatPoint
    {
        [JsonProperty("x")] public float X;
        [JsonProperty("y")] public float Y;
    }

    // gridData 单格
    public class GridCellData
    {
        public int X;
        public int Y;
        public int State;
        public int PathIndex;

        // 编辑器写回的路段起点/终点标记：运行时忽略，供关卡编辑器保存后回读
        [JsonProperty("isStart")] public bool IsStart;
        [JsonProperty("isEnd")] public bool IsEnd;

        // 路径点自定义世界坐标（网格坐标，不含 OrigionPos）：编辑器里对单点做过 xyz 微调才写，
        // 缺省时运行时回退到格子中心 (X+0.5, 0, Y+0.5)
        [JsonProperty("pX")] public float? PX;
        [JsonProperty("pY")] public float? PY;
        [JsonProperty("pZ")] public float? PZ;

        // state == 1 表示路径格
        public bool IsRoad => State == 1;
    }

    // 容器格子数据，装车子
    public class HolderData
    {
        [JsonProperty("x")] public float X;
        [JsonProperty("y")] public float Y;
        [JsonProperty("width")] public int Width;
        [JsonProperty("height")] public int Height;
        [JsonProperty("direction")] public int? Direction;
        [JsonProperty("directionValue")] public float? DirectionValue;
        [JsonProperty("gameUnits")] public List<GameUnitData> GameUnits;
        [JsonProperty("states")] public List<GameUnitStateData> States;
        // JSON 字段名沿用 HolderType，但语义是"基础外壳类型"，映射到 HolderPrefabType；特殊玩法在 States 里
        [JsonProperty("HolderType")] public int? PrefabType;
    }

    /// <summary>
    /// 单个车子的数据，包含基础颜色与特殊玩法状态
    /// 对应配置 JSON 中 gameUnits 数组内单条单元，完整结构示例：
    /// <code>
    /// "gameUnits":
    /// {
    ///   "ID": {
    ///     "Color": 9
    ///   },
    ///   "States": [
    ///     {
    ///       "type": 5,
    ///       "ID": {
    ///         "stateType": 5,
    ///         "idType": 1
    ///       },
    ///       "ColorType": 12
    ///     }
    ///   ]
    /// }
    /// </code>
    /// <para>容器本身也可能带特殊玩法：HolderData 的 states 数组（同 GameUnitStateData 结构）</para>
    /// <para>分层结构说明：</para>
    /// <para>• <b> GameUnitData </b>：顶层根节点，代表一整辆车，持有基础外观ID + 所有特殊状态集合 </para>
    /// <para>• <b> GameUnitIdData </b>：车辆本体标识，仅存储颜色编号，区分车型外观 </para>
    /// <para>• <b> GameUnitStateData </b>：单条特殊玩法状态，type 代表状态大类，嵌套状态专属配置 </para>
    /// <para>• <b> GameUnitStateIdData </b>：特殊状态专属参数，存放细分状态类型、子ID标识，和车辆基础ID做隔离 </para>
    /// </summary>
    public class GameUnitData
    {
        [JsonProperty("ID")] public GameUnitIdData Id;
        [JsonProperty("States")] public List<GameUnitStateData> States;

        /// <summary>从 States 解析车辆玩法类型（type/stateType=1 表示问号玩法）。</summary>
        public VehicleType GetVehicleType()
        {
            if (States == null || States.Count == 0) return VehicleType.Normal;

            foreach (var state in States)
            {
                if (state.Type == (int)VehicleType.Hidden
                    || state.Id?.StateType == (int)VehicleType.Hidden)
                    return VehicleType.Hidden;
            }

            return VehicleType.Normal;
        }
    }

    // 车子基础标识数据，常规仅存储颜色
    public class GameUnitIdData
    {
        [JsonProperty("Color")] public int? Color;
    }

    // 单条特殊玩法状态外层数据
    public class GameUnitStateData
    {
        [JsonProperty("type")] public int Type;
        [JsonProperty("ID")] public GameUnitStateIdData Id;
        [JsonProperty("ColorType")] public int? ColorType;
        [JsonProperty("Durability")] public int? Durability;
    }

    // 特殊玩法细分参数数据
    public class GameUnitStateIdData
    {
        [JsonProperty("stateType")] public int? StateType;
        [JsonProperty("idType")] public int? IdType;
    }

    #endregion
}