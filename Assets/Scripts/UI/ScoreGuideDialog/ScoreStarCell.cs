using System;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public class ScoreStarCell : AttachPoolPrefabBase
    {
        [SerializeField] private Image image;
        private int _index;
        private Action<int> _onClick;
        private AtlasLoader _atlasLoader;

        public void Init(int index, Action<int> onClick, AtlasLoader atlasLoader)
        {
            _index = index;
            _onClick = onClick;
            _atlasLoader = atlasLoader;

            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            ChangeState(false);
        }

        public void ChangeState(bool isSelected)
        {
            _atlasLoader.LoadSprite(image, Constant.AtlasId.ScoreGuide, isSelected ? "icon_star_light" : "icon_star_dark");
        }

        public void OnClick()
        {
            _onClick?.Invoke(_index);
        }
    }
}