using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class HolderHighlightController : MonoBehaviour
    {
        private static HolderHighlightController _instance;

        public static HolderHighlightController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<HolderHighlightController>();
                    if (_instance == null)
                    {
                        var go = new GameObject(nameof(HolderHighlightController));
                        _instance = go.AddComponent<HolderHighlightController>();
                    }
                }

                return _instance;
            }
        }

        public bool IsActive => gameObject.activeSelf;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            gameObject.SetActive(false);
        }

        public void SetHighlight(bool on)
        {
            gameObject.SetActive(on);
        }
    }
}