using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 容器特殊玩法统一契约
    /// </summary>
    public interface ISpecialMachine
    {
        // 初始化
        void Init(Holder holder, GameUnitStateData state);

        void OnSetup(CoffeeAwayCore core);

        // 进度广播
        void UpdateProgress();
        bool CanAcceptVehicle(ColorType colorType = ColorType.None);

        bool CanPopExit();

        // 是否允许点击容器
        bool CanTap();
        void DeSpawn();
    }

    /// <summary>
    /// 容器特殊玩法组件基类。挂在容器 GameObject 上与 Holder 同级
    /// </summary>
    public abstract class HolderSpecialCtrl : AttachPoolPrefabBase, ISpecialMachine
    {
        [SerializeField] private Animator _animator;

        protected Animator Animator => _animator;
        protected AnimatorStateInfo AnimatorStateInfo => _animator.GetCurrentAnimatorStateInfo(0);
        protected Holder selfHolder { get; private set; }
        protected GameUnitStateData Config { get; private set; }

        public virtual void Init(Holder holder, GameUnitStateData state)
        {
            selfHolder = holder;
            Config = state;
        }

        public virtual bool CanAcceptVehicle(ColorType colorType = ColorType.None) => true;
        public virtual bool CanPopExit() => true;
        public virtual bool CanTap() => true;

        public virtual void UpdateProgress()
        {
        }

        // 关卡初始化完成后注入核心
        public virtual void OnSetup(CoffeeAwayCore core)
        {
        }
    }
}