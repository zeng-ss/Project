using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 逐行放大强调效果 - 每一行单独放大强调，一行一行播放，然后循环往复
/// </summary>
public class UIFormTextView_CycleLineScale : UIFormTextViewBase
{
    // floatValue[0]: 放大倍数（默认1.3）
    // floatValue[1]: 单行动画时长（默认0.4秒）
    // floatValue[2]: 行之间的延迟（默认0.2秒）
    // easePresets[0]: 缩放缓动曲线（默认OutQuad）

    private float GetScaleMultiplier => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : 1.3f;
    private float GetLineDuration => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : 0.4f;
    private float GetLineDelay => UIFormTextSo.floatValue.Count > 2 ? UIFormTextSo.floatValue[2] : 0.2f;
    private Ease GetScaleEase => UIFormTextSo.easePresets.Count > 0 ? UIFormTextSo.easePresets[0] : Ease.OutQuad;

    private Vector3[][] _originalVertices;
    private TMP_TextInfo _textInfo;
    private Dictionary<int, List<int>> _lineToCharacters; // 行号 -> 该行的字符索引列表
    private float[] _lineProgress; // 每行的动画进度

    public UIFormTextView_CycleLineScale(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
    {
    }

    public override void PlayText(Sequence sequence)
    {
        if (TMP_Text == null) return;

        // 强制更新网格信息
        TMP_Text.ForceMeshUpdate();

        _textInfo = TMP_Text.textInfo;
        int characterCount = _textInfo.characterCount;

        if (characterCount == 0) return;

        // 保存原始顶点数据
        _originalVertices = new Vector3[_textInfo.meshInfo.Length][];
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _originalVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
        }

        // 识别所有行并分组字符
        GroupCharactersByLine();

        // 初始化行进度数组
        int lineCount = _lineToCharacters.Count;
        _lineProgress = new float[lineCount];

        // 为每一行创建动画
        CreateLineScaleAnimation(sequence, lineCount);

        // 设置无限循环，每轮结束后延迟一小段时间再开始下一轮
        sequence.AppendInterval(0.2f);
        sequence.SetLoops(-1, LoopType.Restart);
    }

    /// <summary>
    /// 将字符按行分组
    /// </summary>
    private void GroupCharactersByLine()
    {
        _lineToCharacters = new Dictionary<int, List<int>>();

        for (int i = 0; i < _textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[i];

            // 跳过不可见的字符
            if (!charInfo.isVisible) continue;

            // 获取字符所在的行号
            int lineNumber = charInfo.lineNumber;

            // 如果该行不存在，创建新的列表
            if (!_lineToCharacters.ContainsKey(lineNumber))
            {
                _lineToCharacters[lineNumber] = new List<int>();
            }

            // 将字符索引添加到对应行的列表中
            _lineToCharacters[lineNumber].Add(i);
        }
    }

    /// <summary>
    /// 为每一行创建缩放动画
    /// </summary>
    private void CreateLineScaleAnimation(Sequence sequence, int lineCount)
    {
        int lineIndex = 0;
        foreach (var kvp in _lineToCharacters)
        {
            int lineNumber = kvp.Key;
            List<int> characterIndices = kvp.Value;

            // 为该行创建缩放动画
            int currentLineIndex = lineIndex;
            Tween lineTween = DOTween.To(() => 0f, (float progress) =>
            {
                _lineProgress[currentLineIndex] = progress;
                UpdateLineScale(lineNumber, progress);
            }, 1f, GetLineDuration)
            .SetEase(GetScaleEase);

            // 使用 Append 顺序添加动画，实现一行一行播放的效果
            sequence.Append(lineTween);

            // 如果不是最后一行，添加行之间的延迟
            if (lineIndex < lineCount - 1)
            {
                sequence.AppendInterval(GetLineDelay);
            }

            lineIndex++;
        }
    }

    /// <summary>
    /// 更新指定行的缩放，同时重置其他行的状态
    /// </summary>
    private void UpdateLineScale(int lineNumber, float progress)
    {
        if (TMP_Text == null || _textInfo == null) return;
        if (!_lineToCharacters.ContainsKey(lineNumber)) return;

        // 计算当前缩放值 (使用正弦曲线实现 1 -> 峰值 -> 1)
        // progress: 0->1, 使用sin使其 0->峰值->0
        float scaleOffset = Mathf.Sin(progress * Mathf.PI) * (GetScaleMultiplier - 1f);
        float currentScale = 1f + scaleOffset;

        // 更新所有行的字符状态
        foreach (var kvp in _lineToCharacters)
        {
            int currentLineNumber = kvp.Key;
            List<int> characterIndices = kvp.Value;
            
            // 确定当前行的缩放值
            float lineScale = (currentLineNumber == lineNumber) ? currentScale : 1f;

            // 更新该行所有字符的顶点
            foreach (int charIndex in characterIndices)
            {
                if (charIndex >= _textInfo.characterCount) continue;
                
                TMP_CharacterInfo charInfo = _textInfo.characterInfo[charIndex];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                // 确保索引有效
                if (materialIndex >= _textInfo.meshInfo.Length || 
                    materialIndex >= _originalVertices.Length) continue;
                if (vertexIndex + 3 >= _originalVertices[materialIndex].Length) continue;

                Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;

                // 计算字符中心点（基于原始顶点）
                Vector3 charCenter = (_originalVertices[materialIndex][vertexIndex] +
                                      _originalVertices[materialIndex][vertexIndex + 1] +
                                      _originalVertices[materialIndex][vertexIndex + 2] +
                                      _originalVertices[materialIndex][vertexIndex + 3]) / 4f;

                // 更新4个顶点的位置（相对于中心点缩放）
                for (int j = 0; j < 4; j++)
                {
                    if (vertexIndex + j >= _originalVertices[materialIndex].Length) continue;
                    
                    Vector3 originalPos = _originalVertices[materialIndex][vertexIndex + j];
                    Vector3 direction = originalPos - charCenter;
                    vertices[vertexIndex + j] = charCenter + direction * lineScale;
                }
            }
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
        _lineToCharacters = null;
        _lineProgress = null;
    }
}
