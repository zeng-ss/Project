using System.Collections.Generic;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// CarSort 道具管理器：字典注册表分发道具 + Shuffle 选目标与高亮。
    /// </summary>
    public class PropMgr
    {
        private readonly CoffeeAwayCore _core;
        private readonly Dictionary<ItemType, PropBase> _props = new();

        private ItemType? _pendingTarget;

        public bool IsSelectingTarget => _pendingTarget != null;

        public PropMgr(CoffeeAwayCore core)
        {
            _core = core;
            Register(new UndoProp(core));
            Register(new ShuffleProp(core));
            Register(new ExpandRoadProp(core));
            Register(new EliminateColorProp(core));
        }

        // 注册表：重复注册 fail-fast
        private void Register(PropBase prop) => _props.Add(prop.ItemType, prop);

        public void Reset()
        {
            CancelSelection();
        }

        // 按道具类型统一调用。Shuffle 无 target 时进入高亮。返回是否消耗道具
        public bool UsePropByType(ItemType type, Holder targetHolder = null, ColorType targetColor = default)
        {
            // 自动完成 / 满载判定中禁止使用道具，避免破坏必赢局面或与死局判定冲突
            if (_core.IsAutoCompleting) return false;
            if (!_props.TryGetValue(type, out var prop)) return false;

            if (type == ItemType.Shuffle)
            {
                if (!_core.ShouldShowShuffleHighlight())
                {
                    GameEntry.UI.OpenUIForm(UIFormType.GlobalToast,
                        UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("93")));
                    return false;
                }

                if (targetHolder is null)
                    return BeginShuffleSelection();
            }

            if (!prop.Execute(targetHolder, targetColor)) return false;

            if (type is ItemType.Shuffle or ItemType.EliminateColor)
                _core.UndoHistory.Clear();

            return true;
        }

        /// <summary>选目标阶段点击 Holder；若已消费返回 true。</summary>
        public bool TrySelectHolder(Holder holder)
        {
            if (_pendingTarget != ItemType.Shuffle || holder is null) return false;
            if (!holder.ShouldHighlightForShuffle()) return false;
            UseShuffle(holder);
            return true;
        }

        private bool BeginShuffleSelection()
        {
            _pendingTarget = ItemType.Shuffle;
            HolderHighlightController.Instance.SetHighlight(true);
            _core.RefreshAllRenderLayers();
            _core.StopAllVehicles();
            return true;
        }

        private void CancelSelection()
        {
            _pendingTarget = null;
            HolderHighlightController.Instance?.SetHighlight(false);
            _core.RefreshAllRenderLayers();
        }

        private bool UseShuffle(Holder holder)
        {
            if (!UsePropByType(ItemType.Shuffle, holder)) return false;

            CancelSelection();
            return true;
        }
    }
}