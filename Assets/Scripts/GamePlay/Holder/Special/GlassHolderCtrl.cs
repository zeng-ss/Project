using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    // 玻璃容器特殊玩法：完成指定数量(Durability)的容器后，玻璃罩解锁开盖，方可进出车辆
    public class GlassHolderCtrl : HolderSpecialCtrl
    {
        [SerializeField] private TMP_Text _glassText;

        private CoffeeAwayCore _core;
        private bool _locked = true;
        private int curCompleteNum; // 当前已完成的容器数量
        private int totalCompleteNum; // 需要完成的容器数量
        private static readonly string IdleState = "DrinkCover_Idle";
        private static readonly string OpenState = "DrinkCover_Open";

        public override void Init(Holder holder, GameUnitStateData state)
        {
            base.Init(holder, state);
            transform.localPosition = Vector3.zero;
            Animator.gameObject.transform.localPosition = Vector3.zero;
            _locked = true;
            _core = null;
            curCompleteNum = 0;
            if (Config.Durability != null) totalCompleteNum = Config.Durability.Value;
            else
            {
                totalCompleteNum = 1;
                Log.Error("[GlassHolderCtrl.Init()] Config.Durability is invalid.");
            }

            //PlayState(IdleState);
            _glassText.gameObject.SetActive(true);
            // TMP 可读面是 -Z：直接复制相机旋转（billboard 朝向），文字正面朝向相机
            // 不能用 LookAt(camera.position)（+Z 指向相机导致文字镜像），LookAt(-camera.forward) 把方向当世界坐标更错
            _glassText.transform.rotation = Camera.main.transform.rotation;
            _glassText.text = totalCompleteNum.ToString();
        }

        public override void Spawn()
        {
            Animator.gameObject.transform.localPosition = Vector3.zero;
        }

        // 关卡初始化完成后注入核心
        public override void OnSetup(CoffeeAwayCore core) => _core = core;

        public override bool CanAcceptVehicle(ColorType colorType = ColorType.None) => !_locked;
        public override bool CanPopExit() => !_locked;
        public override bool CanTap() => !_locked;

        // 外部调用：任意容器完成时由 CoffeeAwayCore.NotifyContainersCompleted 广播触发
        public override void UpdateProgress()
        {
            base.UpdateProgress();
            if (!_locked || selfHolder is null) return; // 已解锁的玻璃不再重复计数

            curCompleteNum++;
            if (curCompleteNum >= totalCompleteNum)
            {
                Unlock();
                return;
            }

            _glassText.text = (totalCompleteNum - curCompleteNum).ToString();
        }

        private void Update()
        {
            if (IsOpenAnimDone())
            {
                selfHolder?.RemoveSpecial(this);
                SpawnFactory.HolderPool.DeSpawn(this);
            }
        }

        private void Unlock()
        {
            _locked = false;
            Log.Info("[GlassHolderCtrl.Unlock()] 玻璃容器解锁");
            PlayState(OpenState);
            _core?.RefreshAllRenderLayers();
        }

        // 开盖动画播完后销毁组件，容器恢复正常玩法
        private bool IsOpenAnimDone()
        {
            if (_locked) return false;
            if (Animator is null) return true;
            var info = Animator.GetCurrentAnimatorStateInfo(0);
            return info.IsName(OpenState) && info.normalizedTime >= 0.95f;
        }

        private void PlayState(string stateName)
        {
            Animator?.Play(stateName, 0, 0f);
        }

        public override void DeSpawn()
        {
            _locked = true;
            _core = null;
            curCompleteNum = 0;
            totalCompleteNum = 0;
            Animator.gameObject.transform.localPosition = Vector3.zero;
            base.DeSpawn();
        }
    }
}
