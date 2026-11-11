using Custom.Activity;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace CasualGame
{
    public class HomePanelEntrance : AttachPoolPrefabBase, IEffectTarget
    {
        [SerializeField] protected Button _button;
        [SerializeField] protected TextMeshProUGUI _activityName;
        [SerializeField] protected Image _activityIcon;
        [SerializeField] private SkeletonGraphic _activitySpine;
        [SerializeField] private Image _redPoint;
        [SerializeField] private Material[] _uiMaterials;
        [SerializeField] private Image _lockIcon;
        protected IHomeActivity _activity;
        protected ActivityType _activityType;
        protected float lastParticlePlayTime = -1f;
        public ActivityType ActivityType => _activityType;
        public Image Icon => _activityIcon;
        private SkeletonDataAsset _skeletonDataAsset;
        protected CancellationTokenSource _cancellationTokenSource;
        protected virtual bool RealUnlcok => _activity.ShowEntry && (_activity as IActivity).IsActivated;
        private const string SPINE_PATH = "Assets/Game/Spines/Screw_Icon";

        protected virtual void Awake()
        {
            EventManager.AddListener(Constant.Event.UpdateBtnRedPoint, OnUpdateBtnRedPoint);
            EventManager.AddListener(Constant.Event.UpdateBtnActive, OnUpdateBtnActive);
        }

        protected virtual void OnDestroy()
        {
            EventManager.RemoveListener(Constant.Event.UpdateBtnRedPoint, OnUpdateBtnRedPoint);
            EventManager.RemoveListener(Constant.Event.UpdateBtnActive, OnUpdateBtnActive);
        }

        public void SetActivity(IHomeActivity activity)
        {
            _activity = activity;
            _activity.OnSpawnEntrance(this);
            _activityType = (activity as IActivity).ActivityType;
            gameObject.name = _activityType.ToString();
            ControlGameObjectActive(true);
        }

        public override void Spawn()
        {
            _button.onClick.AddListener(OnClick);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void DeSpawn()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            if (_skeletonDataAsset != null)
            {
                GameEntry.Resource.UnloadAsset(_skeletonDataAsset);
                _skeletonDataAsset = null;
            }
            _button.onClick.RemoveListener(OnClick);
            _activity.OnDespawnEntrance(this);
        }

        protected virtual void OnClick()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);

            if (RealUnlcok)
                _activity.ClickActivityUI();
            else
            {
                GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("655", _activity.ActivityInfo.UnlockLv)));
            }
        }

        public void RefreshTime()
        {
            if (_activity == null || !_activity.ShouldRefreshTime()) return;

            IActivity iActivity = _activity as IActivity;
            if (iActivity == null) return;

            RefreshActivityName();
        }

        protected virtual void RefreshActivityName()
        {
            if (!_activity.ShowEntry) return;

            if ((_activity as IActivity).IsActivated)
            {
                int key = _activity.GetTextKey();
                if (key > 0)
                {
                    _activityName.text = GameEntry.Localization.GetString(key.ToString()).Replace("\n", "").ToUpper();
                }
                else
                {
                    _activityName.text = _activity.GetEntranceName();
                }
            }
            else if (_activity.ActivityInfo.ConditionType == (int)ActivityConditionType.ByTime && _activity.ActivityInfo.IsLevelCondition(ProxyManager.UserProxy.GameLevel))
            {
                _activityName.text = _activity.GetEntranceName();
            }
            else
            {
                _activityName.text = $"Lv.{_activity.ActivityInfo.UnlockLv}";
            }
        }

        public virtual async UniTaskVoid RefreshUI(AtlasLoader loader)
        {
            RefreshActivityName();

            DRActivityInfo info = _activity.ActivityInfo;
            string spinePath = !string.IsNullOrEmpty(info.SpinePath) && info.SpinePath != "-1" ? SPINE_PATH + "_" + info.SpinePath : null;
            if (spinePath != null)
            {
                _activitySpine.gameObject.SetActive(false);
                _skeletonDataAsset = await GameEntry.Resource.LoadAsset<SkeletonDataAsset>(spinePath + "/Icon.asset", _cancellationTokenSource.Token);
                _activitySpine.skeletonDataAsset = _skeletonDataAsset;

                _activitySpine.gameObject.SetActive(true);

                _activitySpine.Initialize(true);

                SetGray(!RealUnlcok);

                _activitySpine.AnimationState.ClearTracks();
                _activitySpine.Skeleton.SetToSetupPose();
                TrackEntry entry = _activitySpine.AnimationState.SetAnimation(0, "Idle", true);
                entry.TrackTime = Random.Range(0f, entry.Animation.Duration);
            }

            _lockIcon.gameObject.SetActive(!RealUnlcok);
        }

        private void SetGray(bool isGray)
        {
            _activitySpine.material = isGray ? _uiMaterials[1] : _uiMaterials[0];
        }

        private void OnUpdateBtnRedPoint(CustomEventArgs args)
        {
            if (_activity == null || _redPoint == null)
                return;

            UpdateBtnRedPointEventArgs redArgs = args as UpdateBtnRedPointEventArgs;
            ActivityType mine = (_activity as IActivity).ActivityType;
            if (redArgs != null && redArgs.activityType != ActivityType.None && redArgs.activityType != mine)
                return;

            _redPoint.gameObject.SetActive(_activity.RedPointCount() > 0 && RealUnlcok);
        }

        private void OnUpdateBtnActive(CustomEventArgs args)
        {
            ControlGameObjectActive(_activity.ShowEntry);
        }

        private void ControlGameObjectActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public virtual Transform GetTransform()
        {
            return transform;
        }


        public virtual void PlayParticle()
        {
            if (Time.time - lastParticlePlayTime < 0.1f)
            {
                return;
            }

            _activitySpine.transform.localScale = Vector3.one;
            _activitySpine.transform.DOKill();
            _activitySpine.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                _activitySpine.transform.DOScale(1f, 0.1f);
            });
            lastParticlePlayTime = Time.time;
        }
    }
}