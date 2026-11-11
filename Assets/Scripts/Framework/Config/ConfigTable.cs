using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// 一张配置表的内存容器：按 Id 查询，或取全部行。
    /// 数据由 JsonMgr 从 StreamingAssets/*.json 反序列化而来。
    /// </summary>
    public class ConfigTable<T> where T : class
    {
        private readonly List<T> _rows;
        private readonly Dictionary<int, T> _map = new();
        private T[] _array;

        public ConfigTable(List<T> rows, Func<T, int> idSelector)
        {
            _rows = rows ?? new List<T>();
            foreach (T row in _rows)
            {
                _map[idSelector(row)] = row;
            }
        }

        /// <summary>行数。</summary>
        public int Count => _rows.Count;

        /// <summary>按 Id 取一行，不存在返回 null。</summary>
        public T GetDataRow(int id) => _map.TryGetValue(id, out T row) ? row : null;

        /// <summary>是否存在指定 Id 的行。</summary>
        public bool HasDataRow(int id) => _map.ContainsKey(id);

        /// <summary>取全部行（返回缓存数组，不要修改）。</summary>
        public T[] GetAllDataRows() => _array ??= _rows.ToArray();
    }
}