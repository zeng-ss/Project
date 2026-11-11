using System.Collections.Generic;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 撤销道具：回退玩家上一步可撤销操作。
    /// </summary>
    public class UndoProp : PropBase
    {
        public override ItemType ItemType => ItemType.Undo;

        public UndoProp(CoffeeAwayCore core) : base(core) { }

        public override bool Execute(Holder targetHolder = null, ColorType targetColor = default)
        {
            return Core.UndoHistory.TryUndo(Core);
        }
    }
    public class UndoHistory
    {
        private const int MaxDepth = 20;
        private readonly List<UndoRecord> _undoRecords = new();

        public bool CanUndo => _undoRecords.Count > 0;

        public void Clear() => _undoRecords.Clear();

        public void PushMove(UndoRecord record)
        {
            _undoRecords.Add(record);
            if (_undoRecords.Count > MaxDepth)
                _undoRecords.RemoveAt(0);
        }

        public bool TryUndo(CoffeeAwayCore core)
        {
            if (core is null || _undoRecords.Count == 0)
            {
                GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("2")));
                return false;
            }

            var record = _undoRecords[_undoRecords.Count - 1];
            if (!core.CanApplyUndo(record))
            {
                GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("2")));
                return false;
            }

            core.ApplyUndo(record);
            _undoRecords.RemoveAt(_undoRecords.Count - 1);
            return true;
        }
    }
}
