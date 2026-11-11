using UnityEngine;
using TMPro;
using CasualGame;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DotTextAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private string baseTextId;
    [SerializeField] private float dotInterval = 0.5f;
    [SerializeField] private int maxDots = 3;

    private float _timer;
    private int _currentDots;
    private string _baseText;

    private void Awake()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }
        _baseText = textComponent.text;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= dotInterval)
        {
            _timer = 0f;
            _currentDots = (_currentDots + 1) % (maxDots + 1);
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (textComponent == null) return;

        if (string.IsNullOrEmpty(baseTextId))
        {
            textComponent.text = _baseText + new string('.', _currentDots);
        }
        else
        {
            textComponent.text = LocalizationMgr.Instance.GetString(baseTextId) + new string('.', _currentDots);
        }
    }
}