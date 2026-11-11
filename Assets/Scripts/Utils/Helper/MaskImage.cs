using System.Collections.Generic;
using CasualGame;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public static class ImageHelper
{
    public static Vector4 bounds(this Image ui)
    {
        Rect rect = ui.rectTransform.rect;
        return new Vector4(rect.yMax, rect.xMax, rect.yMin, rect.xMin);
    }

    public static Vector4 OuterUV(this Image ui)
    {
        return ui.overrideSprite != null ? DataUtility.GetOuterUV(ui.overrideSprite) : Vector4.zero;
    }

    public static Bounds GetBounds(this Transform _targetTransform, Transform father, out int state)
    {
        if (_targetTransform == null)
        {
            state = 2;
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        if (_targetTransform is RectTransform rect)
        {
            state = 1;
            return RectTransformUtility.CalculateRelativeRectTransformBounds(father, _targetTransform);
        }
        else
        {
            var target = new Bounds(_targetTransform.position, Vector3.zero);
            MeshRenderer[] mf = _targetTransform.GetComponentsInChildren<MeshRenderer>(true);
            if (mf != null && mf.Length > 0)
            {
                foreach (var m in mf)
                {
                    target.Encapsulate(m.bounds);
                }

                state = 0;
                return target;
            }
            else
            {
                SpriteRenderer[] sf = _targetTransform.GetComponentsInChildren<SpriteRenderer>(true);
                if (sf != null && sf.Length > 0)
                {
                    foreach (var sr in sf)
                    {
                        target.Encapsulate(sr.bounds);
                    }
                    state = 0;
                    return target;
                }
            }
        }

        state = 2;
        return new Bounds(Vector3.zero, Vector3.zero);
    }

    public static List<Vector2> GetUVPos(this List<Vector3> v3s, Image ui)
    {
        var uv = ui.OuterUV();
        var bound = ui.bounds();
        var L = bound.w - bound.y;
        var H = bound.z - bound.x;
        var uvL = uv.z - uv.x;
        var uvH = uv.w - uv.y;
        List<Vector2> temp = new List<Vector2>();
        foreach (var it in v3s)
        {
            var l = it.x - bound.w;
            var h = it.y - bound.z;
            temp.Add(new Vector2(l / L * uvL, h / H * uvH));
        }

        return temp;
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(MaskImage), true)]
public class ImageUIEditor : ImageEditor
{
    SerializedProperty _sprite;
    SerializedProperty _transform;


    protected override void OnEnable()
    {
        base.OnEnable();

        this._sprite = this.serializedObject.FindProperty("m_Sprite");
        this._transform = this.serializedObject.FindProperty("_targetTransform");
    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();

        this.SpriteGUI();
        this.AppearanceControlsGUI();
        this.RaycastControlsGUI();
        bool showNativeSize = this._sprite.objectReferenceValue != null;
        this.m_ShowNativeSize.target = showNativeSize;
        this.MaskableControlsGUI();
        this.NativeSizeButtonGUI();
        EditorGUILayout.PropertyField(this._transform);
        // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("targetBtn"));
        // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("snakePoint"));
        // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("topRound"));
        // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("bottomRound"));
        // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("time"));
        this.serializedObject.ApplyModifiedProperties();
    }
}
#endif
public class MaskImage : Image
{

    [SerializeField] private Transform _targetTransform;
    // public RectTransform targetBtn;

    public void SetTarget(Transform target, Bounds bounds, int cstate, bool isUpdate = false)
    {
        _targetTransform = target;
        curBounds = bounds;
        this.state = cstate;
        this.isUpdate = isUpdate;
        Refresh();
    }

    public void Refresh()
    {
        Rebuild(CanvasUpdate.Layout);
    }

    public void Hide()
    {
        // targetBtn.gameObject.SetActive(false);
    }

    private void Update()
    {
        // if (targetBtn.gameObject.activeSelf != btnActive)
        // {
        //     targetBtn.gameObject.SetActive(btnActive);
        // }

        // if (btnActive)
        // {
        //     var temp = maxTemp - minTemp;
        //     targetBtn.anchoredPosition = (maxTemp + minTemp) / 2f;
        //     targetBtn.sizeDelta = temp;
        // }
    }

    private Vector2 minTemp, maxTemp;
    private bool btnActive = false;
    private Bounds curBounds = new Bounds();
    private bool isUpdate = false;
    private int state = -1;

    [ContextMenu("Refresh")]
    public void Refresh1()
    {
        SetTarget(_targetTransform, new Bounds(), state, true);
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
        Bounds bound = curBounds;
        if (isUpdate)
        {
            bound = _targetTransform.GetBounds(transform, out state);
        }

        Vector2 min = Vector2.zero, max = Vector2.zero;
        switch (state)
        {
            case 0:
                Vector3 toMax = bound.max - bound.min;
                Vector3 vx = bound.min + new Vector3(toMax.x, 0, 0);
                Vector3 vy = bound.min + new Vector3(0, toMax.y, 0);
                Vector3 vz = bound.min + new Vector3(0, 0, toMax.z);
                Vector3 vxz = bound.min + new Vector3(toMax.x, 0, toMax.z);
                Vector3 vxy = vx + new Vector3(0, toMax.y, 0);
                Vector3 vzy = vz + new Vector3(0, toMax.y, 0);
                List<Vector3> posList = new List<Vector3>() { bound.min, vx, vz, vxz, vxy, vzy, vy, bound.max };
                List<Vector2> list = new List<Vector2>();
                foreach (var pos in posList)
                {
                    list.Add(TransformUtility.WorldToScreenPoint(pos));
                }

                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                foreach (var scrPos in list)
                {
                    if (scrPos.x > maxX)
                    {
                        maxX = scrPos.x;
                    }

                    if (scrPos.x < minX)
                    {
                        minX = scrPos.x;
                    }

                    if (scrPos.y < minY)
                    {
                        minY = scrPos.y;
                    }

                    if (scrPos.y > maxY)
                    {
                        maxY = scrPos.y;
                    }
                }

                Vector2 minScreen = new Vector2(minX, minY);
                Vector2 maxScreen = new Vector2(maxX, maxY);
                TransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, minScreen, Camera.main, out min);
                TransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, maxScreen, Camera.main, out max);
                break;
            case 1:
                min = bound.min;
                max = bound.max;
                break;
            default:
                btnActive = false;
                break;
        }

        List<int> tri = new List<int>();
        var color32 = color;
        RectTransform rectTran = transform as RectTransform;
        Rect rect = rectTran.rect;
        var temp = (rect.max - rect.min);
        float rh = temp.y;
        float rl = temp.x;
        Vector3 f1 = rect.min;
        Vector3 f2 = rect.min + new Vector2(0, rh);
        Vector3 f3 = rect.max;
        Vector3 f4 = rect.min + new Vector2(rl, 0);
        var temp1 = max - min;
        minTemp = min;
        maxTemp = max;
        float h = temp1.y;
        float l = temp1.x;

        bool isNull = transform == null;
        Vector3 f5 = isNull ? (f1 + f3) * 0.5f : min;
        Vector3 f6 = isNull ? (f1 + f3) * 0.5f : min + new Vector2(0, h);
        Vector3 f7 = isNull ? (f1 + f3) * 0.5f : max;
        Vector3 f8 = isNull ? (f1 + f3) * 0.5f : min + new Vector2(l, 0);
        List<Vector3> v3s = new List<Vector3>();
        v3s.Add(f1);
        v3s.Add(f2);
        v3s.Add(f3);
        v3s.Add(f4);
        v3s.Add(f5);
        v3s.Add(f6);
        v3s.Add(f7);
        v3s.Add(f8);
        var uvs = v3s.GetUVPos(this);
        for (int i = 0; i < v3s.Count; i++)
        {
            toFill.AddVert(v3s[i], color32, uvs[i]);
        }

        toFill.AddTriangle(0, 1, 5);
        toFill.AddTriangle(0, 5, 4);
        toFill.AddTriangle(1, 6, 5);
        toFill.AddTriangle(1, 2, 6);
        toFill.AddTriangle(2, 7, 6);
        toFill.AddTriangle(2, 3, 7);
        toFill.AddTriangle(3, 0, 7);
        toFill.AddTriangle(7, 0, 4);
        btnActive = true;
    }
}