using System;
using System.Collections;
using System.Collections.Generic;
using CasualGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using DG.Tweening;

[DisallowMultipleComponent]
public class LocalizationForUndeal : MonoBehaviour
{
    public string localizationKey;
    public bool isOffEffect = false;
    public UIFormTextSo uIFormTextSo;
    /// <summary>
    /// 是否启用文字变化监听（文字重绘时自动重新应用效果）
    /// </summary>
    public bool enableTextChangeListener = false;
    public StrReplace[] replaceList;
    public string appendSignal;//后面附加的符号

    private UIFormTextViewBase uIFormTextViewBase;
    private Text text;
    private TextMeshProUGUI TextMeshText;
    private TextMeshPro meshPro;
    private bool hasListenEvent = false;
    private TMP_Text _currentTmpText;
    private string _currentKey;
    private bool _isListening = false;
    private Sequence tempSeq;
    private CancellationTokenSource cancelToken;
    private bool isChanging = false;

    public string LocalizationKey
    {
        set
        {
            localizationKey = value;
            RefreshText();
        }
    }
    private void Awake()
    {
        text = GetComponent<Text>();
        TextMeshText = GetComponent<TextMeshProUGUI>();
        meshPro = GetComponent<TextMeshPro>();
    }

    private void RefreshLocalText(CustomEventArgs args)
    {
        RefreshText();
    }

    private void Start()
    {
        cancelToken = new CancellationTokenSource();
        EventManager.AddListener(Constant.Event.LanguageChangeSuccess, RefreshLocalText);
        hasListenEvent = true;

        RefreshText();

    }

    private void RefreshText()
    {
        TMP_Text tMP_Text = null;
        if (text != null)
        {
            var localization = GetLocalization(localizationKey);
            if (localization != null)
            {
                text.text = localization;
            }
        }
        else if (TextMeshText != null)
        {

            var localization = GetLocalization(localizationKey);
            if (localization != null)
            {
                TextMeshText.text = localization;
            }
            tMP_Text = TextMeshText;
        }
        else if (meshPro != null)
        {
            var localization = GetLocalization(localizationKey);
            if (meshPro != null)
            {
                meshPro.text = localization;
            }
            tMP_Text = meshPro;
        }

        if (tMP_Text != null && !isOffEffect)
        {
            _currentTmpText = tMP_Text;
            _currentKey = localizationKey;
            StopSeq();
            StopTextEffect();
            PlayTextEffect(tMP_Text, localizationKey);

            // 启用文字变化监听
            if (enableTextChangeListener && !_isListening)
            {
                RegisterTextChangeListener();
            }
        }
    }

    private string GetLocalization(string localizationKey)
    {

        if (string.IsNullOrEmpty(localizationKey))
        {
            Log.Error("{0}/{1}", transform.parent.name, transform.name);
            return null;
        }

        var str = LocalizationMgr.Instance.GetString(localizationKey);
        if (replaceList != null && replaceList.Length > 0)
        {
            foreach (var replace in replaceList)
            {
                str = str.Replace(replace.holder, replace.placement);
            }
        }

        if (!string.IsNullOrEmpty(appendSignal))
        {
            str += appendSignal;
        }
        return str;
    }

    /// <summary>
    /// 注册文字变化监听
    /// </summary>
    private void RegisterTextChangeListener()
    {
        if (_currentTmpText == null) return;

        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        _isListening = true;
    }

    /// <summary>
    /// 文字变化回调
    /// </summary>
    private void OnTextChanged(UnityEngine.Object obj)
    {
        // 检查是否是当前文本组件触发的变化
        if (obj == _currentTmpText && _currentTmpText != null)
        {
            if (isChanging)
            {
                return;
            }
            // 延迟一帧重新应用效果，确保文字布局已完成
            StartCoroutine(ReapplyEffectNextFrame());
        }
    }

    /// <summary>
    /// 下一帧重新应用效果
    /// </summary>
    private System.Collections.IEnumerator ReapplyEffectNextFrame()
    {
        isChanging = true;
        yield return null;

        if (_currentTmpText != null && !string.IsNullOrEmpty(_currentKey))
        {
            StopTextEffect();
            StopSeq();
            PlayTextEffect(_currentTmpText, _currentKey);
        }
        isChanging = false;
    }

    private void PlayTextEffect(TMP_Text tMP_Text, string key)
    {
        if (uIFormTextSo == null)
        {
            return;
        }

        tempSeq = UIFormTextHelper.PlayTextEffect(key, tMP_Text, uIFormTextSo);
        if (tempSeq == null)
        {
            return;
        }

        if (cancelToken == null || cancelToken.IsCancellationRequested)
        {
            StopSeq();
        }
    }

    private void StopSeq()
    {
        if (tempSeq != null && tempSeq.IsActive())
        {
            tempSeq.Kill();
        }
    }
    private void StopTextEffect()
    {
        // 清除TMP_Text上的所有文字效果
        if (_currentTmpText != null)
        {
            // 清除UIFormTextHelper中的效果记录
            UIFormTextHelper.ClearTextEffect(_currentTmpText);
            // 停止所有相关的DOTween动画
            DOTween.Kill(_currentTmpText);
            // 重置网格到默认状态
            _currentTmpText.ForceMeshUpdate();
        }

        // 兼容旧代码
        if (uIFormTextViewBase != null)
        {
            uIFormTextViewBase.ResetDefault();
            uIFormTextViewBase = null;
        }
    }

    /// <summary>
    /// 取消注册文字变化监听
    /// </summary>
    private void UnregisterTextChangeListener()
    {
        if (_isListening)
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
            _isListening = false;
        }
    }

    private void OnDestroy()
    {
        if (hasListenEvent)
        {
            EventManager.RemoveListener(Constant.Event.LanguageChangeSuccess, RefreshLocalText);
            hasListenEvent = false;
        }

        UnregisterTextChangeListener();
        StopTextEffect();
        StopSeq();

        if (cancelToken != null)
        {
            cancelToken?.Cancel();
            cancelToken?.Dispose();
            cancelToken = null;
        }
    }

    public void RefreshTextEffect()
    {
        RefreshText();
    }

#if UNITY_EDITOR
    [ContextMenu("RefreshLanguage")]
    public void Refresh()
    {
        RefreshText();
    }

#endif

    [Serializable]
    public class StrReplace
    {
        public string holder;
        public string placement;
    }
}
