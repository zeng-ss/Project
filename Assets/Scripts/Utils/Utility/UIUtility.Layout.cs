using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static partial class UIUtility
{
    public static void Horizontal<T>(this List<T> list, TextAnchor textAnchor, float space = 10f,
        Vector4? border = null) where T : MonoBehaviour
    {
        if (list.Count <= 0) return;

        List<RectTransform> rectTransforms = new List<RectTransform>();

        for (int i = 0; i < list.Count; i++)
        {
            rectTransforms.Add(list[i].GetComponent<RectTransform>());
        }

        Horizontal(rectTransforms, list[0].transform.parent.GetComponent<RectTransform>(), textAnchor,
            space, border);
    }

    public static void Vertical<T>(this List<T> list, TextAnchor textAnchor, float space) where T : MonoBehaviour
    {
        if (list.Count <= 0) return;

        List<RectTransform> rectTransforms = new List<RectTransform>();

        for (int i = 0; i < list.Count; i++)
        {
            rectTransforms.Add(list[i].GetComponent<RectTransform>());
        }

        Vertical(rectTransforms, list[0].transform.parent.GetComponent<RectTransform>(), textAnchor,
            space);
    }

    public static void Grid<T>(this List<T> list, RectTransform parentRectTransform,
        Vector2? space = null, Vector4? border = null, Vector2Int? count = null) where T : MonoBehaviour
    {
        if (list.Count <= 0) return;

        List<RectTransform> rectTransforms = new List<RectTransform>();

        for (int i = 0; i < list.Count; i++)
        {
            rectTransforms.Add(list[i].GetComponent<RectTransform>());
        }

        int totalCount = rectTransforms.Count;
        if (totalCount <= 0) return;

        List<Vector2> result = GridData(parentRectTransform, rectTransforms[0], totalCount, space, border, count);

        for (var index = 0; index < rectTransforms.Count; index++)
        {
            var rectTransform = rectTransforms[index];
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = result[index];
        }
    }

    // border:左上右下
    private static void Horizontal(List<RectTransform> rectTransforms, RectTransform parentRectTransform,
        TextAnchor textAnchor, float space, Vector4? border = null)
    {
        Vector4 temBorder = new Vector4();
        if (border != null)
        {
            temBorder = border.Value;
        }

        float currentWidth = 0;
        currentWidth += temBorder.x;

        float totalWidth = temBorder.x + temBorder.z - space;
        foreach (var rectTransform in rectTransforms)
        {
            bool ignore = !rectTransform.gameObject.activeSelf;
            if (ignore) continue;

            totalWidth += space + rectTransform.rect.width;
        }

        if (textAnchor == TextAnchor.MiddleCenter || textAnchor == TextAnchor.UpperCenter)
        {
            currentWidth = -(totalWidth / 2f);
        }

        foreach (var rectTransform in rectTransforms)
        {
            bool ignore = !rectTransform.gameObject.activeSelf;

            if (ignore) continue;

            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);

            Vector2 tempSizeDelta = rectTransform.sizeDelta;
            Vector2 tempPivot = rectTransform.pivot;
            float x = 0, y = 0;

            switch (textAnchor)
            {
                case TextAnchor.UpperLeft:
                    x = currentWidth + rectTransform.sizeDelta.x * tempPivot.x;
                    y = -tempSizeDelta.y * tempPivot.y - temBorder.y;
                    currentWidth += tempSizeDelta.x + space;
                    break;
                case TextAnchor.UpperCenter:
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    x = currentWidth + rectTransform.sizeDelta.x * (1 - tempPivot.x);
                    y = -tempSizeDelta.y * tempPivot.y;
                    currentWidth += tempSizeDelta.x + space;
                    break;
                case TextAnchor.MiddleLeft:
                    x = currentWidth + rectTransform.sizeDelta.x * tempPivot.x;
                    y = -(parentRectTransform.rect.size.y - tempSizeDelta.y) / 2f - tempSizeDelta.y * (1 - tempPivot.y);
                    currentWidth += tempSizeDelta.x + space;
                    break;
                case TextAnchor.MiddleCenter:
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    x = currentWidth + rectTransform.sizeDelta.x * (1 - tempPivot.x);
                    y = 0;
                    currentWidth += tempSizeDelta.x + space;
                    break;
                case TextAnchor.MiddleRight:
                {
                    rectTransform.anchorMin = new Vector2(1f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    x = -totalWidth + (currentWidth + rectTransform.sizeDelta.x * tempPivot.x);
                    y = 0;
                    currentWidth += rectTransform.rect.width + space;
                    break;
                }
                case TextAnchor.LowerLeft:
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.anchorMin = new Vector2(0, 0);
                    x = currentWidth + rectTransform.sizeDelta.x * tempPivot.x;
                    y = tempSizeDelta.y * tempPivot.y;
                    currentWidth += tempSizeDelta.x + space;
                    break;
            }

            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }

    private static void Vertical(List<RectTransform> rectTransforms, RectTransform parentRectTransform,
        TextAnchor textAnchor, float space)
    {
        float currentHeight = 0;
        float left = 0;
        float right = 0;

        if (textAnchor == TextAnchor.MiddleCenter || textAnchor == TextAnchor.MiddleLeft)
        {
            currentHeight = 0;
            float totalLength = 0;
            for (int i = 0; i < rectTransforms.Count; i++)
            {
                RectTransform rectTransform = rectTransforms[i];
                bool ignore = !rectTransform.gameObject.activeSelf;
                if (ignore) continue;

                if (i != 0) totalLength += space;

                totalLength += rectTransform.sizeDelta.y;
            }

            currentHeight -= totalLength / 2f;
        }

        for (int i = 0; i < rectTransforms.Count; i++)
        {
            RectTransform rectTransform = rectTransforms[i];

            Vector2 temSizeDelta = rectTransform.sizeDelta;
            bool ignore = !rectTransform.gameObject.activeSelf;

            if (ignore) continue;

            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            Vector2 pivot = rectTransform.pivot;
            float x = 0, y = 0;

            switch (textAnchor)
            {
                case TextAnchor.UpperCenter:
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    x = 0;
                    y = -currentHeight - temSizeDelta.y * (1 - pivot.y);
                    currentHeight += temSizeDelta.y + space;
                    break;
                case TextAnchor.UpperLeft:
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.anchorMin = new Vector2(0, 1);
                    x = left + temSizeDelta.x * pivot.x;
                    y = -currentHeight - temSizeDelta.y * (1 - pivot.y);
                    currentHeight += temSizeDelta.y + space;
                    break;
                case TextAnchor.UpperRight:
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.anchorMin = new Vector2(1, 1);
                    x = -right - temSizeDelta.x * (1 - pivot.x);
                    y = -currentHeight - temSizeDelta.y * (1 - pivot.y);
                    currentHeight += temSizeDelta.y + space;
                    break;
                case TextAnchor.MiddleCenter:
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    x = 0;
                    y = -currentHeight - temSizeDelta.y * (1 - pivot.y);
                    currentHeight += temSizeDelta.y + space;
                    break;
                case TextAnchor.LowerCenter:
                {
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    x = 0;
                    y = currentHeight + temSizeDelta.y * (1 - pivot.y);
                    currentHeight += temSizeDelta.y + space;
                    break;
                }
                case TextAnchor.LowerRight:
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.anchorMin = new Vector2(1, 0);
                    x = -right - rectTransform.sizeDelta.x * (1 - pivot.x);
                    y = currentHeight + temSizeDelta.y * pivot.y;
                    currentHeight += temSizeDelta.y + space;
                    break;
            }

            rectTransform.anchoredPosition3D = new Vector3(x, y, 0);
        }
    }

    // border:左上右下
    public static List<Vector2> GridData(RectTransform parentRectTransform, RectTransform tempCell, int totalCount,
        Vector2? space = null, Vector4? border = null, Vector2Int? count = null)
    {
        List<Vector2> result = new List<Vector2>();

        Vector2 spacing = space ?? Vector2.zero;
        Vector4 borderValue = border ?? Vector4.zero;

        float width = parentRectTransform.rect.width;
        width -= borderValue.x + borderValue.z;
        float height = parentRectTransform.rect.height;
        height -= borderValue.y + borderValue.w;

        // 取第一个当做模板
        Vector2 cellSize = tempCell.sizeDelta;
        Vector2 pivot = tempCell.pivot;

        Vector2Int countVale = count ?? new Vector2Int(0, 0);

        int column = countVale.x > 0 ? countVale.x : Mathf.FloorToInt((width + spacing.x) / (cellSize.x + spacing.x));

        column = Mathf.Max(1, column);
        int row = Mathf.CeilToInt(totalCount / (float)column);

        float totalWidth = column * cellSize.x + (column - 1) * spacing.x;
        float totalHeight = row * cellSize.y + (row - 1) * spacing.y;

        float startX = -totalWidth / 2 + cellSize.x / 2;
        float startY = totalHeight / 2 - cellSize.y / 2;

        for (int i = 0; i < totalCount; i++)
        {
            int rowIndex = i / column;
            int columnIndex = i % column;

            float widthOfThisRow = Mathf.Min(column, totalCount - rowIndex * column) * cellSize.x +
                                   Mathf.Max(0, Mathf.Min(column - 1, totalCount - rowIndex * column - 1)) *
                                   spacing.x;
            float startXOfThisRow = -widthOfThisRow / 2 + cellSize.x / 2;

            float x = startXOfThisRow + columnIndex * (cellSize.x + spacing.x) - 0.5f * cellSize.x +
                      cellSize.x * pivot.x;
            x += borderValue.x;

            float y = startY - rowIndex * (cellSize.y + spacing.y) - 0.5f * cellSize.y + cellSize.y * pivot.y;
            y -= borderValue.y;

            result.Add(new Vector2(x, y));
        }

        return result;
    }

    public static void ContentSize(this RectTransform rectTransform, int axis, ContentSizeFitter.FitMode fitMode)
    {
        // Set size to min or preferred size
        if (fitMode == ContentSizeFitter.FitMode.MinSize)
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis,
                LayoutUtility.GetMinSize(rectTransform, axis));
        else
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis,
                LayoutUtility.GetPreferredSize(rectTransform, axis));
    }
}