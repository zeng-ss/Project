using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 推拉门文字效果：字符从左侧滑入，新出现的字符先和已出现文字稍微重叠，然后归位
/// </summary>
public class UIFormTextView_SlideDoorIn : UIFormTextViewBase
{
    // floatValue[0]: 水平偏移量（默认-30，从左侧滑入的距离）
    // floatValue[1]: 单个字符动画时长（默认0.25秒）
    // floatValue[2]: 字符之间的延迟（默认0.04秒）
    // floatValue[3]: 重叠距离（默认-15，字符超过目标位置后回弹的距离，负值表示向左偏移）
    // easePresets[0]: 动画缓动曲线（默认OutBack）

    private float GetSlideOffset => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : -30f;
    private float GetAnimDuration => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : 0.25f;
    private float GetCharDelay => UIFormTextSo.floatValue.Count > 2 ? UIFormTextSo.floatValue[2] : 0.04f;
    private float GetOverlapOffset => UIFormTextSo.floatValue.Count > 3 ? UIFormTextSo.floatValue[3] : -15f;
    private Ease GetAnimEase => UIFormTextSo.easePresets.Count > 0 ? UIFormTextSo.easePresets[0] : Ease.OutBack;

    // 记录每个字符的动画进度
    private float[] _charProgress;
    private Vector3[][] _originalVertices;
    private Color32[][] _originalColors;
    private TMP_TextInfo _textInfo;
    private int _characterCount;

    public UIFormTextView_SlideDoorIn(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
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

        // 初始化字符进度数组
        _charProgress = new float[_characterCount];

        // 保存原始顶点和颜色数据
        _originalVertices = new Vector3[_textInfo.meshInfo.Length][];
        _originalColors = new Color32[_textInfo.meshInfo.Length][];
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _originalVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
            _originalColors[i] = (Color32[])_textInfo.meshInfo[i].colors32.Clone();
        }

        // 先隐藏所有字符（设置透明度为0并移到左侧）
        HideAllCharacters();

        // 为每个字符创建滑入动画
        CreateSlideInAnimation(sequence);
    }

    /// <summary>
    /// 隐藏所有字符（设置透明度为0并移到左侧）
    /// </summary>
    private void HideAllCharacters()
    {
        for (int i = 0; i < _characterCount; i++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = _textInfo.meshInfo[materialIndex].colors32;

            // 将所有顶点移到左侧并设置透明
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _originalVertices[materialIndex][vertexIndex + j];
                vertices[vertexIndex + j] = originalPos + new Vector3(GetSlideOffset, 0, 0);
                colors[vertexIndex + j] = new Color32(
                    _originalColors[materialIndex][vertexIndex + j].r,
                    _originalColors[materialIndex][vertexIndex + j].g,
                    _originalColors[materialIndex][vertexIndex + j].b,
                    0);
            }

            _charProgress[i] = 0f;
        }

        // 更新网格
        TMP_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void CreateSlideInAnimation(Sequence sequence)
    {
        int visibleCharIndex = 0;
        for (int i = 0; i < _characterCount; i++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[i];

            // 跳过不可见的字符
            if (!charInfo.isVisible) continue;

            int charIndex = i;
            float startDelay = visibleCharIndex * GetCharDelay;
            visibleCharIndex++;

            // 创建字符滑入动画
            Tween charTween = DOTween.To(() => 0f, (float progress) =>
            {
                _charProgress[charIndex] = progress;
                UpdateAllCharacters();
            }, 1f, GetAnimDuration)
            .SetEase(GetAnimEase);

            // 使用 Insert 在指定时间点插入动画
            sequence.Insert(startDelay, charTween);
        }
    }

    /// <summary>
    /// 计算水平偏移（从左侧滑入，先超过目标位置再回弹归位）
    /// </summary>
    private float CalculateSlideOffset(float progress)
    {
        // 推拉门效果：
        // 阶段1：0-0.65 快速从左侧滑入到超过目标位置（向右偏移，即正值）
        // 阶段2：0.65-1.0 从超过目标位置回弹重叠到目标位置

        if (progress < 0.65f)
        {
            // 从 SlideOffset 滑入到超过目标（OverlapOffset，负值表示重叠）
            float t = progress / 0.65f;
            // 使用 OutQuad 曲线实现快速到达
            t = 1f - (1f - t) * (1f - t);
            return Mathf.Lerp(GetSlideOffset, GetOverlapOffset, t);
        }
        else
        {
            // 从重叠位置回弹到目标位置（0）
            float t = (progress - 0.65f) / 0.35f;
            // 使用平滑曲线
            t = t * t * (3f - 2f * t); // smoothstep
            return Mathf.Lerp(GetOverlapOffset, 0f, t);
        }
    }

    /// <summary>
    /// 更新所有字符的变换
    /// </summary>
    private void UpdateAllCharacters()
    {
        if (TMP_Text == null) return;

        // 刷新文本信息以获取最新的布局
        TMP_Text.ForceMeshUpdate();

        // 重新保存当前的原始顶点数据（布局可能已改变）
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _originalVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
            _originalColors[i] = (Color32[])_textInfo.meshInfo[i].colors32.Clone();
        }

        for (int charIndex = 0; charIndex < _characterCount; charIndex++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[charIndex];
            if (!charInfo.isVisible) continue;

            float progress = _charProgress[charIndex];

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = _textInfo.meshInfo[materialIndex].colors32;

            // 计算当前X轴偏移（使用自定义滑动曲线）
            float currentOffsetX = CalculateSlideOffset(progress);

            // 计算当前缩放（轻微的缩放变化，从0.9到1.02再到1）
            float currentScale;
            if (progress < 0.5f)
            {
                float t = progress / 0.5f;
                t = 1f - (1f - t) * (1f - t);
                currentScale = Mathf.Lerp(0.9f, 1.02f, t);
            }
            else
            {
                float t = (progress - 0.5f) / 0.5f;
                currentScale = Mathf.Lerp(1.02f, 1f, t);
            }

            // 计算当前透明度（快速淡入）
            float alphaProgress = Mathf.Clamp01(progress * 4f); // 前25%时间内完成淡入
            byte currentAlpha = (byte)Mathf.Lerp(0, 255, alphaProgress);

            // 计算字符中心点（基于当前原始位置）
            Vector3 charCenter = (_originalVertices[materialIndex][vertexIndex] +
                                  _originalVertices[materialIndex][vertexIndex + 1] +
                                  _originalVertices[materialIndex][vertexIndex + 2] +
                                  _originalVertices[materialIndex][vertexIndex + 3]) / 4f;

            // 更新4个顶点的位置和颜色
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _originalVertices[materialIndex][vertexIndex + j];

                // 相对于中心点的位置
                Vector3 relativePos = originalPos - charCenter;

                // 应用缩放
                relativePos *= currentScale;

                // 应用X轴偏移
                Vector3 offset = new Vector3(currentOffsetX, 0, 0);

                // 计算最终位置
                vertices[vertexIndex + j] = charCenter + relativePos + offset;

                // 更新颜色透明度
                colors[vertexIndex + j] = new Color32(
                    _originalColors[materialIndex][vertexIndex + j].r,
                    _originalColors[materialIndex][vertexIndex + j].g,
                    _originalColors[materialIndex][vertexIndex + j].b,
                    currentAlpha);
            }
        }

        // 更新网格（顶点和颜色）
        TMP_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
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
        _charProgress = null;
        _originalVertices = null;
        _originalColors = null;
        _textInfo = null;
    }
}
