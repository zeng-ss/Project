using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 弧形弯曲文字效果 - 让标题文字沿着扇形弧线弯曲排列（无动画，直接生效）
/// 自动适应不同语言和字符宽度，使用实际字符宽度计算间距
/// </summary>
public class UIFormTextView_ArcCurve : UIFormTextViewBase
{
    // floatValue[0]: 弧形半径（默认500，值越大弧度越平缓）
    // floatValue[1]: 弧形方向（默认-1向下凹，1向上凸）
    // 注意：间距自动根据字符实际宽度计算，无需手动配置

    private float GetArcRadius => UIFormTextSo.floatValue.Count > 0 ? UIFormTextSo.floatValue[0] : 500f;
    private float GetArcDirection => UIFormTextSo.floatValue.Count > 1 ? UIFormTextSo.floatValue[1] : -1f;

    private Vector3[][] _originalVertices;
    private TMP_TextInfo _textInfo;
    private int _characterCount;

    public UIFormTextView_ArcCurve(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text) : base(uIFormTextSo, tMP_Text)
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

        // 直接应用弧形效果（无动画）
        ApplyArcCurve();
    }

    /// <summary>
    /// 应用弧形弯曲效果 - 基于实际字符宽度自动计算间距
    /// </summary>
    private void ApplyArcCurve()
    {
        if (TMP_Text == null) return;

        // 弧形参数
        float arcRadius = GetArcRadius;
        float direction = GetArcDirection >= 0 ? 1f : -1f;

        // 使用TMP的文本边界中心，这样会尊重原始的对齐设置
        Bounds textBounds = TMP_Text.textBounds;
        float centerX = textBounds.center.x;
        float centerY = textBounds.center.y;

        // 弧形圆心位置
        Vector3 arcCenter = new Vector3(centerX, centerY - arcRadius * direction, 0f);

        // 中心角度
        float centerAngle = 90f * direction;

        // 遍历每个字符
        for (int i = 0; i < _characterCount; i++)
        {
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[materialIndex].vertices;

            // 获取字符的4个顶点
            // TMP顶点顺序: 0=左下, 1=左上, 2=右上, 3=右下
            Vector3 bottomLeft = _originalVertices[materialIndex][vertexIndex + 0];
            Vector3 topLeft = _originalVertices[materialIndex][vertexIndex + 1];
            Vector3 topRight = _originalVertices[materialIndex][vertexIndex + 2];
            Vector3 bottomRight = _originalVertices[materialIndex][vertexIndex + 3];

            // 计算字符的水平中心（使用顶部顶点以获得准确的字符宽度）
            float charCenterX = (topLeft.x + topRight.x) / 2f;

            // 使用TMP提供的真实基线位置（baseLine）
            // baseLine是字符的真实排版基线，对于带降部的字母（如g、y、p）也能正确对齐
            Vector3 charBaseline = new Vector3(
                charCenterX,
                charInfo.baseLine,  // 使用TMP的真实基线
                bottomLeft.z
            );

            // 使用字符中心X位置相对于文本中心的距离，转换为弧长
            float distanceFromCenter = charCenterX - centerX;

            // 字符在弧线上的角度（弧度）
            float angleOffset = (-distanceFromCenter / arcRadius) * direction;
            float charAngleRad = (centerAngle * Mathf.Deg2Rad) + angleOffset;

            // 基线位置在弧线上的目标位置
            Vector3 arcPosition = new Vector3(
                arcCenter.x + Mathf.Cos(charAngleRad) * arcRadius,
                arcCenter.y + Mathf.Sin(charAngleRad) * arcRadius,
                charBaseline.z
            );

            // 字符旋转角度（让字符垂直于弧线切线方向）
            float rotationAngle = angleOffset * Mathf.Rad2Deg;

            // 创建旋转矩阵
            Quaternion rotation = Quaternion.Euler(0f, 0f, rotationAngle);

            // 更新4个顶点
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalPos = _originalVertices[materialIndex][vertexIndex + j];

                // 顶点相对于基线位置的偏移
                Vector3 offsetFromBaseline = originalPos - charBaseline;

                // 应用旋转
                Vector3 rotatedOffset = rotation * offsetFromBaseline;

                // 最终位置 = 弧线上的基线位置 + 旋转后的偏移
                vertices[vertexIndex + j] = arcPosition + rotatedOffset;
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
    }
}
