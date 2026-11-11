using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CasualGame.CoffeeAway
{
    /// <summary>关卡JSON反序列化工具，处理gridData增量坐标补全</summary>
    public static class LevelParser
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        public static LevelData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Error("Level JSON is empty.");
                return null;
            }
            try
            {
                var jObj = JObject.Parse(json);
                var gridJArray = jObj["gridData"] as JArray;
                jObj.Remove("gridData");
                var data = jObj.ToObject<LevelData>(JsonSerializer.Create(Settings));
                data.GridData = NormalizeGrid(gridJArray);
                return data;
            }
            catch (Exception ex)
            {
                Log.Error("Level deserialize failed: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>将gridData增量省略坐标补全为绝对XY坐标</summary>
        private static List<GridCellData> NormalizeGrid(JArray gridJArray)
        {
            if (gridJArray == null || gridJArray.Count == 0) return new List<GridCellData>();
            var result = new List<GridCellData>(gridJArray.Count);
            int curX = 0, curY = 0;
            foreach (JToken token in gridJArray)
            {
                if (token["x"] != null)
                    curX = token["x"].Value<int>();
                else if (token["y"] != null)
                    curX = 0;

                if (token["y"] != null)
                    curY = token["y"].Value<int>();

                result.Add(new GridCellData
                {
                    X = curX,
                    Y = curY,
                    State = (int?)token["state"] ?? 0,
                    PathIndex = (int?)token["pathIndex"] ?? 0,
                    IsStart = (bool?)token["isStart"] ?? false,
                    IsEnd = (bool?)token["isEnd"] ?? false,
                    PX = token["pX"] == null ? (float?)null : token["pX"].Value<float>(),
                    PY = token["pY"] == null ? (float?)null : token["pY"].Value<float>(),
                    PZ = token["pZ"] == null ? (float?)null : token["pZ"].Value<float>(),
                });
            }
            return result;
        }
    }
}