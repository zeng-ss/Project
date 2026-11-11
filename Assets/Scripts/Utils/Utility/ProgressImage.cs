using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressImage : Image
{
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (sprite == null)
        {
            base.OnPopulateMesh(toFill);
        }
        else
        {
            // Vector2 min = rectTransform.rect.min;
            // Vector2 max = rectTransform.rect.max;
            GenerateSlicedSprite(toFill);
        }
    }

    /// <summary>
    /// Generate vertices for a 9-sliced Image.
    /// </summary>
    private void GenerateSlicedSprite(VertexHelper toFill)
    {
        Vector2[] s_VertScratch = new Vector2[4];
        Vector2[] s_UVScratch = new Vector2[4];
        if (!hasBorder)
        {
            GenerateSprite(toFill, false);
            return;
        }

        Vector4 outer, inner, padding, border;
        var activeSprite = sprite;
        if (activeSprite != null)
        {
            outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
            inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
            padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
            border = activeSprite.border;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            padding = Vector4.zero;
            border = Vector4.zero;
        }

        Rect rect = GetPixelAdjustedRect();

        Vector4 adjustedBorders = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect);

        padding = padding / multipliedPixelsPerUnit;

        s_VertScratch[0] = new Vector2(padding.x, padding.y);
        s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

        s_VertScratch[1].x = adjustedBorders.x;
        s_VertScratch[1].y = adjustedBorders.y;

        s_VertScratch[2].x = rect.width - adjustedBorders.z;
        s_VertScratch[2].y = rect.height - adjustedBorders.w;

        for (int i = 0; i < 4; ++i)
        {
            s_VertScratch[i].x += rect.x;
            s_VertScratch[i].y += rect.y;
        }

        float range = 1f;
        if (rect.width <= border.x + border.z)
        {
            range = rect.width / (border.x + border.z);
        }
        s_UVScratch[0] = new Vector2(outer.x, outer.y);
        s_UVScratch[1] = new Vector2(Mathf.Lerp(outer.x, inner.x, range), inner.y);
        s_UVScratch[2] = new Vector2(Mathf.Lerp(inner.z, outer.z, 1 - range), inner.w);
        s_UVScratch[3] = new Vector2(outer.z, outer.w);

        toFill.Clear();

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                // if (!m_FillCenter && x == 1 && y == 1)
                //     continue;

                int y2 = y + 1;

                // Check for zero or negative dimensions to prevent invalid quads (UUM-71372)
                if ((s_VertScratch[x2].x - s_VertScratch[x].x <= 0) || (s_VertScratch[y2].y - s_VertScratch[y].y <= 0))
                    continue;

                AddQuad(toFill,
                    new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                    new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                    color,
                    new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                    new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
            }
        }
    }

    private void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
    {
        int startIndex = vertexHelper.currentVertCount;

        for (int i = 0; i < 4; ++i)
            vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin,
        Vector2 uvMax)
    {
        int startIndex = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
        vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
    {
        Rect originalRect = rectTransform.rect;

        for (int axis = 0; axis <= 1; axis++)
        {
            float borderScaleRatio;

            // The adjusted rect (adjusted for pixel correctness)
            // may be slightly larger than the original rect.
            // Adjust the border to match the adjustedRect to avoid
            // small gaps between borders (case 833201).
            if (originalRect.size[axis] != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }

            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }

        return border;
    }

    private void GenerateSprite(VertexHelper vh, bool lPreserveAspect)
    {
        var spriteSize = new Vector2(sprite.rect.width, sprite.rect.height);

        // Covert sprite pivot into normalized space.
        var spritePivot = sprite.pivot / spriteSize;
        var rectPivot = rectTransform.pivot;
        Rect r = GetPixelAdjustedRect();

        if (lPreserveAspect & spriteSize.sqrMagnitude > 0.0f)
        {
            PreserveSpriteAspectRatio1(ref r, spriteSize);
        }

        var drawingSize = new Vector2(r.width, r.height);
        var spriteBoundSize = sprite.bounds.size;

        // Calculate the drawing offset based on the difference between the two pivots.
        var drawOffset = (rectPivot - spritePivot) * drawingSize;

        var color32 = color;
        vh.Clear();

        Vector2[] vertices = sprite.vertices;
        Vector2[] uvs = sprite.uv;
        for (int i = 0; i < vertices.Length; ++i)
        {
            vh.AddVert(
                new Vector3((vertices[i].x / spriteBoundSize.x) * drawingSize.x - drawOffset.x,
                    (vertices[i].y / spriteBoundSize.y) * drawingSize.y - drawOffset.y), color32,
                new Vector2(uvs[i].x, uvs[i].y));
        }

        UInt16[] triangles = sprite.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            vh.AddTriangle(triangles[i + 0], triangles[i + 1], triangles[i + 2]);
        }
    }

    private void PreserveSpriteAspectRatio1(ref Rect rect, Vector2 spriteSize)
    {
        var spriteRatio = spriteSize.x / spriteSize.y;
        var rectRatio = rect.width / rect.height;

        if (spriteRatio > rectRatio)
        {
            var oldHeight = rect.height;
            rect.height = rect.width * (1.0f / spriteRatio);
            rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
        }
        else
        {
            var oldWidth = rect.width;
            rect.width = rect.height * spriteRatio;
            rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
        }
    }
}