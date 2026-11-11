//------------------------------------------------------------
// File : UIGray.cs
// Email: mailto:zhiqiang.yang
// Desc : 置灰脚本
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CasualGame
{
    [DisallowMultipleComponent]
    public class UIGray : MonoBehaviour
    {
        [SerializeField] private bool isGray = false;

        public bool GrayChild = false;

        public bool IsGray
        {
            get
            {
                return isGray;
            }
            set
            {
                if (isGray != value)
                {
                    isGray = value;
                    SetGray(isGray);
                }
            }
        }

        private void Start()
        {
            SetGray(isGray);
        }


        static private Material _defaultGrayMaterial;
        static private Material GrayMaterial
        {
            get
            {
                if (_defaultGrayMaterial == null)
                {
                    _defaultGrayMaterial = new Material(Shader.Find("Custom/UI/Gray"));

                }
                return _defaultGrayMaterial;
            }
        }

        private void SetGray(bool isGray)
        {
            if (GrayChild)
            {
                int i = 0, count = 0;

                Image[] images = transform.GetComponentsInChildren<Image>(true);
                count = images.Length;
                for (i = 0; i < count; i++)
                {
                    Image g = images[i];

                    if (isGray)
                    {
                        g.material = GrayMaterial;
                    }
                    else
                    {
                        g.material = null;
                    }
                }
            }
            else
            {
                Image g = gameObject.GetComponent<Image>();
                if (isGray)
                {
                    g.material = GrayMaterial;
                }
                else
                {
                    g.material = null;
                }
            }

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UIGray))]
    public class UIGrayInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UIGray gray = target as UIGray;

            gray.IsGray = GUILayout.Toggle(gray.IsGray, "编辑器中置灰");
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}


