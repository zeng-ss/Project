namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 车辆状态机：持有当前状态，统一状态流转入口。
    /// </summary>
    public partial class Vehicle
    {
        private VehicleState CurrentState { get; set; } = VehicleState.InHolder;

        private bool IsState(VehicleState state) => CurrentState == state;

        private void ChangeState(VehicleState newState)
        {
            bool wasInHolder = CurrentState == VehicleState.InHolder;
            CurrentState = newState;
            if (newState == VehicleState.InHolder)
                StartIdleIfWanted(); // 进场动画完成才启动呼吸；打乱/落定时车辆本就在 InHolder，直接生效
            else if (wasInHolder)
                StopIdle(); // 离场（排队/移出）立即停呼吸，避免与出场动画的缩放冲突
            if (CoffeeAwayCore.IsHighlightActive)
                CoffeeAwayGame.Instance?.Core?.RefreshAllRenderLayers();
            else
                ApplyRenderLayer();
        }
    }
}