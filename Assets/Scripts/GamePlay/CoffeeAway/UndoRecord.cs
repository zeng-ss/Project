using System.Collections.Generic;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 单次点击 holder 出车的撤销记录：每辆车从源格子出发，撤销时按当前去向还原。
    /// </summary>
    public class UndoRecord
    {
        public Holder SourceHolder { get; set; }
        public List<CarUndoEntry> Cars { get; } = new();
    }

    public class CarUndoEntry
    {
        public Vehicle Vehicle { get; }
        public int SourceCol { get; }
        public int SourceRow { get; }

        public CarUndoEntry(Vehicle vehicle, int sourceCol, int sourceRow)
        {
            Vehicle = vehicle;
            SourceCol = sourceCol;
            SourceRow = sourceRow;
        }
    }
}
