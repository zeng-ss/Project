using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 滚道路 Mesh 的 UV，让带面以指定世界速度滚动。
    /// 车/咖啡仍由 CarSortCore 沿路径移动，带面与车同速同向前移，呈现"被传送带带走"。
    /// 挂到 RoadMesh 上，Init 传入道路总长与车速。纯视觉，不改任何移动逻辑。
    /// 贴图沿路循环已由 RoadMeshBuilder 的 uvScale 烘焙进 Mesh，这里只负责偏移滚动。
    /// </summary>
    public class ConveyorBeltScroll : MonoBehaviour
    {
        private Material _mat;
        private float _tiles; // 整圈铺几个纹理 = round(roadLength / repeatSpacing)
        private float _roadLength;
        private float _offset; // 当前纹理偏移（tile 单位）
        private float _speed;

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        /// <summary>初始化：传入道路总长（RoadNavigator.TotalLength）、带面移动速度（carRoadSpeed）、纹理重复间距。</summary>
        public void Init(float roadLength, float speed, float repeatSpacing = 2f)
        {
            _roadLength = Mathf.Max(0.01f, roadLength);
            _speed = speed;
            _tiles = Mathf.Max(1f, Mathf.Round(_roadLength / Mathf.Max(0.01f, repeatSpacing)));

            _mat = GetComponent<MeshRenderer>().material; // 自动实例化，避免污染共享的 roadMaterial

            // 美术还没提供传送带纹理时，生成一张可循环的临时纹理，让滚动效果立即可见
            bool hasTexture = (_mat.HasProperty(MainTex) && _mat.GetTexture(MainTex) != null)
                           || (_mat.HasProperty(BaseMap) && _mat.GetTexture(BaseMap) != null);
            if (!hasTexture)
            {
                var tex = CreatePlaceholderBeltTexture();
                if (_mat.HasProperty(MainTex)) _mat.SetTexture(MainTex, tex);
                if (_mat.HasProperty(BaseMap)) _mat.SetTexture(BaseMap, tex);
            }
        }

        private void Update()
        {
            if (_mat == null || _speed <= 0f) return;

            // 沿路是 V 轴，offset 每秒变化 speed*_tiles/_roadLength 个 tile。
            // offset 减小使图案朝 +V（车前进方向）移动，呈"被传送带走"；若方向反了把 -= 改成 +=
            _offset -= _speed * Time.deltaTime * _tiles / _roadLength;
            _offset -= Mathf.Floor(_offset); // 归一到 [0,1)
            var o = new Vector2(0f, _offset);
            if (_mat.HasProperty(MainTex)) _mat.SetTextureOffset(MainTex, o);
            if (_mat.HasProperty(BaseMap)) _mat.SetTextureOffset(BaseMap, o);
        }

        private static Texture2D CreatePlaceholderBeltTexture()
        {
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var dark = new Color32(72, 66, 60, 255);
            var light = new Color32(124, 114, 100, 255);
            var pix = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 横向条带：沿路是纹理 V 轴，横向条带滚动时方向最直观；闭环整数 tile 无缝
                    int band = y % 8;
                    pix[y * w + x] = band < 4 ? light : dark;
                }
            }

            tex.SetPixels32(pix);
            tex.Apply();
            return tex;
        }
    }
}
