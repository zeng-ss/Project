using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIFormTextView_CycleCharScale : UIFormTextViewBase
{
    // floatValue[0]: 默认缩放（默认1.2）
    // floatValue[1]: 单个字符放大时长（默认0.3秒）
    // floatValue[2]: 字符之间的延迟（默认0.05秒）
    // easePresets[0]: 放大缓动曲线（默认OutQuad）

    private float GetDefaultScale => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : 1.2f;
    private float GetScaleDuration => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : 0.3f;
    private float GetCharDelay => UIFormTextSo.floatValue.Count > 2 ? UIFormTextSo.floatValue[2] : 0.05f;
    private Ease GetScaleEase => UIFormTextSo.easePresets.Count > 0 ? UIFormTextSo.easePresets[0] : Ease.OutQuad;
    public UIFormTextView_CycleCharScale(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
    {
    }

    public override void PlayText(Sequence sequence)
    {
        if (TMP_Text == null) return;

        // 强制更新网格信息
        TMP_Text.ForceMeshUpdate();

        TMP_TextInfo textInfo = TMP_Text.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // 保存原始顶点数据
        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = (Vector3[])textInfo.meshInfo[i].vertices.Clone();
        }

        // 计算一轮完整波浪的时长
        float waveLoopDuration = GetScaleDuration + (characterCount - 1) * GetCharDelay;

        // 将所有字符动画加入到 sequence 中
        CreateWaveAnimation(sequence, textInfo, originalVertices, characterCount);

        // 设置无限循环，每轮波浪结束后延迟一小段时间再开始下一轮
        sequence.AppendInterval(waveLoopDuration + 0.2f);
        sequence.SetLoops(-1, LoopType.Restart);
    }

    private void CreateWaveAnimation(Sequence sequence, TMP_TextInfo textInfo, Vector3[][] originalVertices, int characterCount)
    {
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 跳过不可见的字符
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // 为每个字符创建缩放动画
            int charIndex = i;
            float startDelay = i * GetCharDelay;

            // 使用 Insert 将动画加入到 sequence 中，这样停止 sequence 就能停止所有动画
            Tween charTween = DOTween.To(() => 0f, (float progress) =>
            {
                UpdateCharacterScale(textInfo, originalVertices, charIndex, progress);
            }, 1f, GetScaleDuration)
            .SetEase(GetScaleEase);

            // 使用 Insert 在指定时间点插入动画（实现延迟效果）
            sequence.Insert(startDelay, charTween);
        }
    }

    private void UpdateCharacterScale(TMP_TextInfo textInfo, Vector3[][] originalVertices, int charIndex, float progress)
    {
        if (TMP_Text == null) return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

        // 计算当前缩放值 (使用正弦曲线实现 1 -> 峰值 -> 1)
        // progress: 0->1, 使用sin使其 0->峰值->0
        float scaleOffset = Mathf.Sin(progress * Mathf.PI) * (GetDefaultScale - 1f);
        float currentScale = 1f + scaleOffset;

        // 计算字符中心点
        Vector3 charCenter = (originalVertices[materialIndex][vertexIndex] +
                              originalVertices[materialIndex][vertexIndex + 1] +
                              originalVertices[materialIndex][vertexIndex + 2] +
                              originalVertices[materialIndex][vertexIndex + 3]) / 4f;

        // 更新4个顶点的位置（相对于中心点缩放）
        for (int j = 0; j < 4; j++)
        {
            Vector3 originalPos = originalVertices[materialIndex][vertexIndex + j];
            Vector3 direction = originalPos - charCenter;
            vertices[vertexIndex + j] = charCenter + direction * currentScale;
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
    }
}
