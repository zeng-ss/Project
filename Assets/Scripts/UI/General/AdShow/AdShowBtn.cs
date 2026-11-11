using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CasualGame
{
    public class AdShowBtn : MonoBehaviour
    {
        [SerializeField] private RectTransform adBtn;
        [SerializeField] private RectTransform normalBtn;

        private void Awake()
        {
            float offsetX = Mathf.Abs(adBtn.anchoredPosition.x);

#if UNITY_ANDROID
            adBtn.gameObject.SetActive(true);
            normalBtn.gameObject.SetActive(true);
            adBtn.anchoredPosition = new Vector2(offsetX, adBtn.anchoredPosition.y);
            normalBtn.anchoredPosition = new Vector2(-offsetX, normalBtn.anchoredPosition.y);
#else
            adBtn.gameObject.SetActive(false);
            normalBtn.gameObject.SetActive(true);
            normalBtn.anchoredPosition = new Vector2(0f, normalBtn.anchoredPosition.y);
#endif
        }
    }
}