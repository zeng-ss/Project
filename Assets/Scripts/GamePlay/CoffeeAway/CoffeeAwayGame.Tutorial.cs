namespace CasualGame.CoffeeAway
{
    public partial class CoffeeAwayGame
    {
        private Holder _guideTarget;

        // 首关指引：手指指向一个可点击容器并强制高亮，点击该容器后恢复渲染并收起手指
        private void TryStartFirstLevelGuide()
        {
            if (ProxyManager.UserProxy.GameLevel != 0) return;

            _guideTarget = PickGuideTarget();
            _guideTarget.ForceHighlight = true;
            HolderHighlightController.Instance?.SetHighlight(true);
            _core.RefreshAllRenderLayers();
            CoffeeAwayHelper.SetFinger(true, _guideTarget);
        }

        private Holder PickGuideTarget()
        {
            return _core.Holders[0];
        }

        // 点击目标容器后：取消高亮并收起手指
        private void TryCompleteFirstLevelGuide(Holder holder)
        {
            if (_guideTarget == null) return;
            if (holder is null || holder != _guideTarget) return;

            var target = _guideTarget;
            _guideTarget = null;
            CoffeeAwayHelper.SetFinger(false, target);
            target.ForceHighlight = false;
            HolderHighlightController.Instance?.SetHighlight(false);
            _core.RefreshAllRenderLayers();
        }

        private void ResetGuide()
        {
            if (_guideTarget is not null)
            {
                CoffeeAwayHelper.SetFinger(false, _guideTarget);
                _guideTarget.ForceHighlight = false;
            }

            _guideTarget = null;
            HolderHighlightController.Instance?.SetHighlight(false);
        }
    }
}
