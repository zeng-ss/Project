using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 水面漂浮文字效果：字符之间有规律的曲线波动，像波浪一样传递
/// 同时叠加次波增加自然感，避免过于机械
/// </summary>
public class UIFormTextView_WaterFloat : UIFormTextViewBase
{
    // floatValue[0]: 垂直浮动幅度（默认4）
    // floatValue[1]: 水平摆动幅度（默认1.5）
    // floatValue[2]: 波浪周期（默认2秒）
    // floatValue[3]: 字符之间的相位差（默认0.15，形成波浪传递效果）
    // floatValue[4]: 旋转幅度（默认2度）
    // easePresets[0]: 浮动缓动曲线（默认InOutSine）

    private float GetVerticalAmplitude => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : 4f;
    private float GetHorizontalAmplitude => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : 1.5f;
    private float GetFloatDuration => UIFormTextSo.floatValue.Count > 2 ? UIFormTextSo.floatValue[2] : 2f;
    private float GetPhaseOffset => UIFormTextSo.floatValue.Count > 3 ? UIFormTextSo.floatValue[3] : 0.15f;
    private float GetRotationAmplitude => UIFormTextSo.floatValue.Count > 4 ? UIFormTextSo.floatValue[4] : 2f;
    private Ease GetFloatEase => UIFormTextSo.easePresets.Count > 0 ? UIFormTextSo.easePresets[0] : Ease.InOutSine;

    private Vector3[][] _originalVertices;
    private TMP_TextInfo _textInfo;
    private int _characterCount;
    private float _time;

    // 每个字符的独立次波参数（增加自然感，但不影响主要的波浪规律）
    private float[] _subPhaseY;
    private float[] _subPhaseX;
    private float[] _subFreqMultiplier;

    public UIFormTextView_WaterFloat(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
    {
    }

    public override void PlayText(Sequence sequence)
    {
        if (TMP_Text == null) return;

        // 强制更新网格信息
        TMP_Text.ForceMeshUpdate();

        _textInfo = TMP_Text.textInfo;
        _characterCount = _textInfo.characterCount;

        if (_characterCount == 0) return;

        // 保存原始顶点数据
        _originalVertices = new Vector3[_textInfo.meshInfo.Length][];
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _originalVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
        }

        // 初始化次波随机参数
        InitializeSubWaveParams();

        // 使用 sequence 来实现持续动画
        _time = 0f;

        // 创建一个无限循环的动画
        Tween floatTween = DOTween.To(() => _time, (float t) =>
        {
            _time = t;
            UpdateAllCharacters();
        }, 1000f, 1000f)
        .SetEase(Ease.Linear);

        sequence.Append(floatTween);
        sequence.SetLoops(-1, LoopType.Restart);
    }

    /// <summary>
    /// 初始化次波参数（只用于增加微小变化，不影响主波浪规律）
    /// </summary>
    private void InitializeSubWaveParams()
    {
        _subPhaseY = new float[_characterCount];
        _subPhaseX = new float[_characterCount];
        _subFreqMultiplier = new float[_characterCount];

        for (int i = 0; i < _characterCount; i++)
        {
            // 次波的随机相位
            _subPhaseY[i] = Random.Range(0f, Mathf.PI * 2f);
            _subPhaseX[i] = Random.Range(0f, Mathf.PI * 2f);
            // 次波的频率倍数（比主波快一些）
            _subFreqMultiplier[i] = Random.Range(2.5f, 3.5f);
        }
    }

    /// <summary>
    /// 更新所有字符的浮动效果
    /// </summary>
    private void UpdateAllCharacters()
    {
        if (TMP_Text == null) return;

        // 刷新文本信息
        TMP_Text.ForceMeshUpdate();

        // 重新保存当前的原始顶点数据
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _originalVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
        }

        float baseFreq = Mathf.PI * 2f / GetFloatDuration;
        int visibleCharIndex = 0;

        for (int charIndex = 0; charIndex < _characterCount; charIndex++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[charIndex];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;

            // 计算字符中心点
            Vector3 charCenter = (_originalVertices[materialIndex][vertexIndex] +
                                  _originalVertices[materialIndex][vertexIndex + 1] +
                                  _originalVertices[materialIndex][vertexIndex + 2] +
                                  _originalVertices[materialIndex][vertexIndex + 3]) / 4f;

            // 主波：使用规律的相位差，形成波浪传递效果
            float mainPhase = visibleCharIndex * GetPhaseOffset * Mathf.PI * 2f;
            float mainWaveY = Mathf.Sin(_time * baseFreq + mainPhase);
            float mainWaveX = Mathf.Sin(_time * baseFreq * 0.7f + mainPhase * 1.2f);
            float mainWaveRot = Mathf.Sin(_time * baseFreq * 0.5f + mainPhase * 0.8f);

            // 次波：增加微小的不规则感（只占20%权重）
            float subWaveY = Mathf.Sin(_time * baseFreq * _subFreqMultiplier[charIndex] + _subPhaseY[charIndex]) * 0.2f;
            float subWaveX = Mathf.Sin(_time * baseFreq * _subFreqMultiplier[charIndex] * 0.8f + _subPhaseX[charIndex]) * 0.2f;

            // 综合计算偏移
            float offsetY = (mainWaveY * 0.8f + subWaveY) * GetVerticalAmplitude;
            float offsetX = (mainWaveX * 0.8f + subWaveX) * GetHorizontalAmplitude;
            float rotation = mainWaveRot * GetRotationAmplitude;

            // 创建旋转矩阵
            Quaternion rotationQuat = Quaternion.Euler(0f, 0f, rotation);

            // 更新4个顶点的位置
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _originalVertices[materialIndex][vertexIndex + j];

                // 相对于中心点的位置
                Vector3 relativePos = originalPos - charCenter;

                // 应用旋转
                relativePos = rotationQuat * relativePos;

                // 应用偏移
                Vector3 offset = new Vector3(offsetX, offsetY, 0);

                // 计算最终位置
                vertices[vertexIndex + j] = charCenter + relativePos + offset;
            }

            visibleCharIndex++;
        }

        // 更新网格
        TMP_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    public override void ResetDefault()
    {
        // 停止所有相关的DOTween动画
        DOTween.Kill(TMP_Text);

        // 重置网格到默认状态
        if (TMP_Text != null)
        {
            TMP_Text.ForceMeshUpdate();
        }

        // 重置变量
        _originalVertices = null;
        _textInfo = null;
        _time = 0f;
        _subPhaseY = null;
        _subPhaseX = null;
        _subFreqMultiplier = null;
    }
}
