using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 盖子容器特殊玩法：盖子上显示目标颜色，其他所有含该颜色的容器铺满后解锁盖子，方可进出车辆
    /// </summary>
    public class ColorLockedHolderCtrl : HolderSpecialCtrl
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private CoffeeAwayCore _core;
        private bool _locked = true;
        private ColorType _lockColor;
        private static readonly string IdleState = "TableCloth_Idle";
        private static readonly string OpenState = "TableCloth_Close";

        public override void Init(Holder holder, GameUnitStateData state)
        {
            base.Init(holder, state);
            transform.localPosition = Vector3.zero;
            Animator.gameObject.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            _locked = true;
            _core = null;
            _lockColor = (ColorType)(state.ColorType ?? 0);
            PlayState(IdleState);
            AtlasLoader atlasLoader = new AtlasLoader();
            atlasLoader.LoadSprite(_spriteRenderer, Constant.AtlasId.CoffeeColor, _lockColor.ToString());
            _spriteRenderer.gameObject.SetActive(true);
        }

        public override void Spawn()
        {
            base.Spawn();
            transform.localPosition = Vector3.zero;
            Animator.gameObject.transform.localPosition = new Vector3(0f, 3.8f, 0f);
        }

        // 关卡初始化完成后注入核心
        public override void OnSetup(CoffeeAwayCore core) => _core = core;

        // 门禁：盖子未解锁时不允许车辆进出、点击
        public override bool CanAcceptVehicle(ColorType colorType = ColorType.None) => !_locked;
        public override bool CanPopExit() => !_locked;
        public override bool CanTap() => !_locked;

        private void Update()
        {
            if (AnimatorStateInfo.IsName(OpenState) && AnimatorStateInfo.normalizedTime >= 0.95f)
            {
                selfHolder?.RemoveSpecial(this);
                SpawnFactory.HolderPool.DeSpawn(this);
                return;
            }

            if (!_locked || _core == null || selfHolder is null) return;
            if (_core.IsColorCompleted(_lockColor, selfHolder)) Unlock();
        }

        private void Unlock()
        {
            _locked = false;
            Log.Info("[ColorLockedHolderCtrl] 盖子解锁");
            PlayState(OpenState);
            _spriteRenderer.gameObject.SetActive(false);
            _core.RefreshAllRenderLayers();
        }

        private void PlayState(string stateName) => Animator.Play(stateName, 0, 0f);

        public override void DeSpawn()
        {
            _locked = true;
            _core = null;
            transform.localPosition = Vector3.zero;
            base.DeSpawn();
        }
    }
}