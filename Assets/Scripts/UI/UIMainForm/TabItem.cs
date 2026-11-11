using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using Custom.Activity;

namespace CasualGame
{
    public class TabItem : AttachPoolPrefabBase
    {
        [SerializeField] private Image _icon, _highLight;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _redPoint;
        private ActivityType _panelType;
        public ActivityType ActivityType => _panelType;
        private Action<ActivityType> _onSelect;

        private void OnClick()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            _onSelect?.Invoke(_panelType);
        }

        public void SetData(ActivityType panelType, Action<ActivityType> onSelect, AtlasLoader atlasLoader)
        {
            _panelType = panelType;
            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            _onSelect = onSelect;
            _highLight.gameObject.SetActive(false);
            _icon.rectTransform.localScale = 0.82f * Vector3.one;
            _icon.rectTransform.anchoredPosition = new Vector3(0f, -151f, 0f);
            _text.transform.localScale = Vector3.zero;
            _text.rectTransform.anchoredPosition = new Vector3(0f, -65f, 0f);
            switch (_panelType)
            {
                case ActivityType.HomePanel:
                    _text.text = LocalizationMgr.Instance.GetString("82").ToUpper();
                    atlasLoader.LoadSprite(_icon, Constant.AtlasId.Home, "icon_home");
                    break;
            }
        }

        public void ShowAnim(bool isShow, float duration = 0.3f)
        {
            _icon.transform.DOKill();
            _text.transform.DOKill();
            _highLight.gameObject.SetActive(isShow);
            if (isShow)
            {
                _icon.transform.DOScale(Vector3.one, duration);
                _icon.rectTransform.DOAnchorPosY(-88.5f, duration);
                _text.transform.DOScale(Vector3.one, duration);
                _text.rectTransform.DOAnchorPosY(-134.5f, duration);
            }
            else
            {
                _icon.transform.DOScale(Vector3.one * 0.82f, duration);
                _icon.rectTransform.DOAnchorPosY(-151f, duration);
                _text.transform.DOScale(Vector3.zero, duration);
                _text.rectTransform.DOAnchorPosY(-151f, duration);
            }
        }

        public void UpdateRedPoint(bool show)
        {
            _redPoint.gameObject.SetActive(show);
        }
    }
}