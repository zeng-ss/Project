using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIFormTextView_RotateJumpIn : UIFormTextViewBase
{
    // floatValue[0]: 旋转角度（默认-45度，从该角度旋转到0）
    // floatValue[1]: 跳跃高度（默认40，从底部跳入）
    // floatValue[2]: 单个字符动画时长（默认0.3秒）
    // floatValue[3]: 字符之间的延迟（默认0.045秒）
    // floatValue[4]: 回弹高度（默认8，跳过目标位置后回弹的高度）
    // easePresets[0]: 保留；本效果内部曲线已固定，外层 tween 使用线性进度避免双重缓动

    private float GetRotationAngle => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : -45f;
    private float GetJumpHeight => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : 40f;
    private float GetAnimDuration => UIFormTextSo.floatValue.Count > 2 ? UIFormTextSo.floatValue[2] : 0.3f;
    private float GetCharDelay => UIFormTextSo.floatValue.Count > 3 ? UIFormTextSo.floatValue[3] : 0.045f;
    private float GetBounceHeight => UIFormTextSo.floatValue.Count > 4 ? UIFormTextSo.floatValue[4] : 8f;
    // 记录每个字符的动画进度
    private float[] _charProgress;
    /// <summary>PlayText 时快照的静止网格，动画中只读、每帧从该快照还原再变形。</summary>
    private Vector3[][] _restVertices;
    private Color32[][] _restColors;
    private TMP_TextInfo _textInfo;
    private int _characterCount;

    public UIFormTextView_RotateJumpIn(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
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

        // 保存静止网格快照（动画全程只读，每帧从该快照还原再应用变换）
        _restVertices = new Vector3[_textInfo.meshInfo.Length][];
        _restColors = new Color32[_textInfo.meshInfo.Length][];
        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _restVertices[i] = (Vector3[])_textInfo.meshInfo[i].vertices.Clone();
            _restColors[i] = (Color32[])_textInfo.meshInfo[i].colors32.Clone();
        }

        // 先隐藏所有字符（设置透明度为0并移到底部）
        HideAllCharacters();

        // 为每个字符创建跳入动画
        CreateJumpInAnimation(sequence);
    }

    /// <summary>
    /// 隐藏所有字符（设置透明度为0并移到底部）
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

            // 将所有顶点移到底部并设置透明
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _restVertices[materialIndex][vertexIndex + j];
                vertices[vertexIndex + j] = originalPos + new Vector3(0, -GetJumpHeight, 0);
                colors[vertexIndex + j] = new Color32(
                    _restColors[materialIndex][vertexIndex + j].r,
                    _restColors[materialIndex][vertexIndex + j].g,
                    _restColors[materialIndex][vertexIndex + j].b,
                    0);
            }

            _charProgress[i] = 0f;
        }

        // 更新网格
        TMP_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void CreateJumpInAnimation(Sequence sequence)
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

            // 创建字符跳入动画
            Tween charTween = DOTween.To(() => 0f, (float progress) =>
            {
                _charProgress[charIndex] = progress;
                UpdateAllCharacters();
            }, 1f, GetAnimDuration)
            // 线性进度，避免与内部跳跃/缩放曲线双重缓动导致顿挫
            .SetEase(Ease.Linear);

            // 使用 Insert 在指定时间点插入动画
            sequence.Insert(startDelay, charTween);
        }
    }

    /// <summary>
    /// 计算跳跃曲线（从底部跳入，超过目标位置后回弹）
    /// </summary>
    private float CalculateJumpOffset(float progress)
    {
        // 使用自定义曲线实现跳跃回弹效果
        // 阶段1：0-0.6 快速从底部跳到超过目标位置
        // 阶段2：0.6-1.0 从超过目标位置回弹到目标位置

        if (progress < 0.6f)
        {
            // 从 -JumpHeight 跳到 +BounceHeight（超过目标）
            float t = progress / 0.6f;
            // 使用 OutQuad 曲线
            t = 1f - (1f - t) * (1f - t);
            return Mathf.Lerp(-GetJumpHeight, GetBounceHeight, t);
        }
        else
        {
            // 从 +BounceHeight 回弹到 0（目标位置）
            float t = (progress - 0.6f) / 0.4f;
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(GetBounceHeight, 0f, t);
        }
    }

    /// <summary>
    /// 更新所有字符的变换
    /// </summary>
    private void UpdateAllCharacters()
    {
        if (TMP_Text == null || _restVertices == null) return;

        // 从静止快照还原整段网格，再逐字叠加动画（避免每帧 ForceMeshUpdate 导致顿挫）
        for (int meshIndex = 0; meshIndex < _textInfo.meshInfo.Length; meshIndex++)
        {
            Vector3[] vertices = _textInfo.meshInfo[meshIndex].vertices;
            Color32[] colors = _textInfo.meshInfo[meshIndex].colors32;
            System.Array.Copy(_restVertices[meshIndex], vertices, vertices.Length);
            System.Array.Copy(_restColors[meshIndex], colors, colors.Length);
        }

        for (int charIndex = 0; charIndex < _characterCount; charIndex++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[charIndex];
            if (!charInfo.isVisible) continue;

            float progress = Mathf.Clamp01(_charProgress[charIndex]);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = _textInfo.meshInfo[materialIndex].colors32;

            // 未开始：保持隐藏状态
            if (progress <= 0f)
            {
                for (int j = 0; j < 4; j++)
                {
                    Vector3 restPos = _restVertices[materialIndex][vertexIndex + j];
                    vertices[vertexIndex + j] = restPos + new Vector3(0, -GetJumpHeight, 0);
                    colors[vertexIndex + j] = new Color32(
                        _restColors[materialIndex][vertexIndex + j].r,
                        _restColors[materialIndex][vertexIndex + j].g,
                        _restColors[materialIndex][vertexIndex + j].b,
                        0);
                }
                continue;
            }

            // 计算字符中心点（基于静止网格）
            Vector3 charCenter = (_restVertices[materialIndex][vertexIndex] +
                                  _restVertices[materialIndex][vertexIndex + 1] +
                                  _restVertices[materialIndex][vertexIndex + 2] +
                                  _restVertices[materialIndex][vertexIndex + 3]) / 4f;

            // 计算当前旋转角度（从初始旋转角度到0，带回弹）
            float rotationProgress = Mathf.Clamp01(progress * 1.5f); // 旋转在动画前2/3完成
            float currentRotation;
            if (rotationProgress < 0.7f)
            {
                // 快速旋转到超过目标（-5度）
                float t = rotationProgress / 0.7f;
                t = 1f - (1f - t) * (1f - t);
                currentRotation = Mathf.Lerp(GetRotationAngle, 5f, t);
            }
            else
            {
                // 回弹到0度
                float t = (rotationProgress - 0.7f) / 0.3f;
                currentRotation = Mathf.Lerp(5f, 0f, t);
            }

            // 计算当前Y轴偏移（使用自定义跳跃曲线）
            float currentOffsetY = CalculateJumpOffset(progress);

            // 缩放：更小起点 + 更高峰值 + SmoothStep，放大感更明显、过渡更顺
            const float scaleStart = 0.36f;
            const float scalePeak = 1.28f;
            const float scaleEnd = 1f;
            const float scalePeakAt = 0.46f;
            float currentScale;
            if (progress < scalePeakAt)
            {
                float t = progress / scalePeakAt;
                t = t * t * (3f - 2f * t);
                currentScale = Mathf.Lerp(scaleStart, scalePeak, t);
            }
            else
            {
                float t = (progress - scalePeakAt) / (1f - scalePeakAt);
                t = t * t * (3f - 2f * t);
                currentScale = Mathf.Lerp(scalePeak, scaleEnd, t);
            }

            // 透明度：与缩放节奏对齐，略拉长淡入更丝滑
            float alphaProgress = Mathf.Clamp01(Mathf.SmoothStep(0f, 1f, progress * 2.2f));
            byte currentAlpha = (byte)Mathf.Lerp(0, 255, alphaProgress);

            // 创建旋转矩阵（围绕字符中心旋转）
            Quaternion rotation = Quaternion.Euler(0f, 0f, currentRotation);

            // 更新4个顶点的位置和颜色
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _restVertices[materialIndex][vertexIndex + j];

                // 相对于中心点的位置
                Vector3 relativePos = originalPos - charCenter;

                // 应用缩放
                relativePos *= currentScale;

                // 应用旋转
                relativePos = rotation * relativePos;

                // 应用Y轴偏移
                Vector3 offset = new Vector3(0, currentOffsetY, 0);

                // 计算最终位置
                vertices[vertexIndex + j] = charCenter + relativePos + offset;

                // 更新颜色透明度
                colors[vertexIndex + j] = new Color32(
                    _restColors[materialIndex][vertexIndex + j].r,
                    _restColors[materialIndex][vertexIndex + j].g,
                    _restColors[materialIndex][vertexIndex + j].b,
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
        _restVertices = null;
        _restColors = null;
        _textInfo = null;
    }
}
