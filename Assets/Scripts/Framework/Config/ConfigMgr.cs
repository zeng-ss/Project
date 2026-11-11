using System;
using System.Collections.Generic;
using CasualGame;

namespace Framework
{
    /// <summary>
    /// 配置表管理器：启动时把全部表读进内存，业务代码按 Id 查询。
    /// 表数据由 LuBan 从 Excel 生成到 Assets/StreamingAssets/*.json。
    /// 新增一张表的步骤：
    ///   1. 在 LubanConfig/MiniTemplate/Datas 里加 Excel，跑 gen.bat
    ///   2. 在 DataTables 下加对应的 DRxxx POCO 类
    ///   3. 在下面 LoadAll 里加一行 Register
    /// </summary>
    public class ConfigMgr
    {
        private readonly Dictionary<Type, object> _tables = new();

        private bool _loaded;

        public void LoadAll()
        {
            if (_loaded) return;
            _loaded = true;

            Register<DRActivityInfo>("tbactivityinfo", r => r.Id);
            Register<DRDialogGuide>("tbdialogguide", r => r.Id);
            Register<DRFingerGuide>("tbfingerguide", r => r.Id);
            Register<DRGuide>("tbguide", r => r.Id);
            Register<DRItem>("tbitem", r => r.Id);
            Register<DRItemIAP>("tbitemiap", r => r.Id);
            Register<DRSound>("tbsound", r => r.Id);
            Register<DRUIForm>("tbuiform", r => r.Id);
        }

        /// <summary>取一张表，未加载返回 null。</summary>
        public ConfigTable<T> GetTable<T>() where T : class
        {
            return _tables.TryGetValue(typeof(T), out object table) ? table as ConfigTable<T> : null;
        }

        private void Register<T>(string fileName, Func<T, int> idSelector) where T : class
        {
            List<T> rows = JsonMgr.Instance.LoadData<List<T>>(fileName);
            var table = new ConfigTable<T>(rows, idSelector);
            _tables[typeof(T)] = table;
            UnityEngine.Debug.Log($"[ConfigMgr] {fileName}: {table.Count} 行");
        }
    }
}
