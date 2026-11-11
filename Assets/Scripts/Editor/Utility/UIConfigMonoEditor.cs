using System.Text;
//------------------------------------------------------------
// File : MainController.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEditor;

namespace CasualGame
{

    [CustomEditor(typeof(UIConfigMono))]
    public class UIConfigMonoEditor : UnityEditor.Editor
    {
        private bool isCreatePanel = false;
        private AspectSafeAreaHelper m_AspectSafeAreaHelper;
        public override void OnInspectorGUI()
        {
            Transform panelTransform = FindPanel();
            if (panelTransform != null)
            {
                isCreatePanel = panelTransform.GetComponent<AspectSafeAreaHelper>() != null;
            }
            isCreatePanel = EditorGUILayout.Toggle("生成自适应安全区域Panel", isCreatePanel);
            EditorGUILayout.LabelField("Image、Text等主件不需要点击事件务必去除raycastTarget");
            EditorGUILayout.LabelField("前缀为【AB_】的对象，点击【Auto Bind】自动绑定");
            UIConfigMono zTarget = (UIConfigMono)target;
            FieldObjectList(zTarget.bindObjectList);
            // SerializedProperty configList = serializedObject.FindProperty("configList");
            // EditorGUILayout.PropertyField(configList, true);
            if (GUILayout.Button("Auto Bind"))
            {
                ClearAll(zTarget);
                AutoBind(zTarget);
            }
            if (GUILayout.Button("Clear All"))
            {
                ClearAll(zTarget);
            }
            OnValidatePanel();
            //保存修改
            serializedObject.ApplyModifiedProperties();
        }


        private void ClearAll(UIConfigMono target)
        {
            if (target.bindObjectList != null)
            {
                target.bindObjectList.Clear();
            }
            EditorUtility.SetDirty(target);
        }


        #region 布局ui组件
        /// <summary>
        /// 布局组件列表
        /// </summary>
        /// <param name="pBindObjList"></param>
        private void FieldObjectList(List<UIBindObject> pBindObjList)
        {
            if (DrawHeader("组件列表", "组件列表", false, false))
            {
                BeginContents(false);

                //--------------------------------------//
                //1.先删除null对象,null对象可能由[-] 删除符号带来
                //--------------------------------------//
                for (int i = (pBindObjList.Count - 1); i >= 0; i--)
                {
                    UIBindObject zBindObj = pBindObjList[i];

                    if (zBindObj == null || zBindObj.obj == null)
                    {
                        pBindObjList.RemoveAt(i);
                        continue;
                    }
                }

                //2.编辑对象
                for (int i = 0; i < pBindObjList.Count; i++)
                {
                    UIBindObject zBindObj = pBindObjList[i];

                    //已经在1.里删除null对象了，这里如果还有null，则是奇怪情况
                    if (zBindObj == null || zBindObj.obj == null)
                    {
                        Debug.LogError("why zBindObj is null!!!");
                        continue;
                    }

                    FieldObject(zBindObj);
                    EditorGUILayout.Space();
                }

                //最后面添加一个null的对象
                UIBindObject zNewBindObj = new UIBindObject();
                FieldObject(zNewBindObj);

                if (zNewBindObj.obj != null)
                {
                    UIConfigMono zTarget = target as UIConfigMono;
                    AddBindObject(zTarget, zNewBindObj);

                    EditorUtility.SetDirty(target);
                }

                EndContents();
            }
        }

        /// <summary>
        /// 布局单个组件
        /// </summary>
        /// <param name="pBindObj"></param>
        private void FieldObject(UIBindObject pBindObj)
        {
            GUILayout.BeginHorizontal();
            if (pBindObj.obj == null)
            {
                //GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f));
                pBindObj.obj = EditorGUILayout.ObjectField(pBindObj.obj, typeof(GameObject), true) as GameObject;
            }
            else
            {
                //[-] 删除符号
                if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f)))
                {
                    pBindObj.obj = null;
                }

                pBindObj.obj = EditorGUILayout.ObjectField(pBindObj.obj, typeof(GameObject), true) as GameObject;

                GUILayout.Label("Tag");
                if (string.IsNullOrEmpty(pBindObj.tag))
                {
                    pBindObj.tag = pBindObj.obj.name;
                }
                pBindObj.tag = EditorGUILayout.TextField(pBindObj.tag);

                //上箭头
                if (GUILayout.Button("\u25B2", "PreButton", GUILayout.Width(20f)))
                {
                    MoveObjectItem(pBindObj, true);
                    return;
                }
                //下箭头
                if (GUILayout.Button("\u25BC", "PreButton", GUILayout.Width(20f)))
                {
                    MoveObjectItem(pBindObj, false);
                    return;
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 提升/下降组件顺位
        /// </summary>
        /// <param name="pBindObj"></param>
        /// <param name="pMoveUp"></param>
        private void MoveObjectItem(UIBindObject pBindObj, bool pMoveUp)
        {
            UIConfigMono zTarget = target as UIConfigMono;
            List<UIBindObject> zList = zTarget.bindObjectList;
            if (zList == null)
                return;

            int zIndex = -1;
            int zCount = zList.Count;

            for (int i = 0; i < zCount; ++i)
            {
                UIBindObject zTake = zList[i];
                if (zTake == pBindObj)
                {
                    zIndex = i;
                    break;
                }
            }

            if (zIndex == -1)
                return;

            if (pMoveUp && zIndex > 0)
            {
                zList.RemoveAt(zIndex);
                zList.Insert(zIndex - 1, pBindObj);
            }
            if (!pMoveUp && zIndex < zCount - 1)
            {
                zList.RemoveAt(zIndex);
                zList.Insert(zIndex + 1, pBindObj);
            }
        }

        #endregion


        #region 本类全局操作方法
        /// <summary>
        /// 添加绑定目标
        /// </summary>
        /// <param name="pBindObj"></param>
        private void AddBindObject(UIConfigMono mono, UIBindObject pBindObj)
        {
            if (pBindObj == null)
            {
#if UNITY_EDITOR
                Debug.LogError("AddBindObject pBingObj is null!");
#endif
                return;
            }
            if (mono.bindObjectList == null)
            {
                mono.bindObjectList = new List<UIBindObject>();
            }

            if (mono.bindObjectList.Contains(pBindObj))
            {
                return;
            }

            for (int i = 0; i < mono.bindObjectList.Count; i++)
            {
                UIBindObject zBindObj = mono.bindObjectList[i];
                if (zBindObj == null)
                {
                    continue;
                }

                if (zBindObj.obj == pBindObj.obj)
                {
                    mono.bindObjectList[i] = pBindObj;
                    return;
                }
            }

            mono.bindObjectList.Add(pBindObj);
        }
        #endregion

        /// <summary>
        /// 自动绑定
        /// </summary>
        /// <param name="target"></param>
        private void AutoBind(UIConfigMono target)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbEvent = new StringBuilder();
            Transform[] allTrans = target.GetComponentsInChildren<Transform>(true);

            List<MonoBehaviour> comps = new List<MonoBehaviour>();

            sb.Append("\t\t#region SerializeField");
            sb.AppendLine();

            sbEvent.Append("\t\t#region SerializeField");
            sbEvent.AppendLine();

            foreach (var item in allTrans)
            {
                if (item.name.StartsWith(UIConfigMono.prefix))
                {
                    UIBindObject zNewBindObj = new UIBindObject();
                    zNewBindObj.tag = item.gameObject.name.Replace(UIConfigMono.prefix, "");
                    zNewBindObj.obj = item.gameObject;
                    AddBindObject(target, zNewBindObj);

                    // Debug.Log(zNewBindObj.tag);
                    comps.Clear();
                    zNewBindObj.obj.GetComponents<MonoBehaviour>(comps);
                    comps.Sort(SortMonoBehaviour);

                    System.Type _tempType = typeof(UnityEngine.RectTransform);
                    if (comps.Count > 0)
                    {
                        _tempType = comps[0].GetType();
                    }

                    sb.AppendFormat("\t\tprivate {0} m_{1} => GetRef<{0}>(\"{1}\");", _tempType.ToString(), zNewBindObj.tag);
                    sb.AppendLine();

                    if (item.gameObject.tag != "Unclick")
                    {
                        if (_tempType == typeof(UnityEngine.UI.Button))
                        {
                            sbEvent
                            .AppendFormat("\t\tprivate void OnClick_{0}()", zNewBindObj.tag)
                            .AppendLine()
                            .AppendLine("\t\t{")
                            .AppendLine("\t\t}")
                            .AppendLine();
                        }
                        else if (_tempType == typeof(UnityEngine.UI.Image))
                        {
                            var img = zNewBindObj.obj.GetComponent<UnityEngine.UI.Image>();
                            if (img.raycastTarget)
                            {
                                sbEvent
                             .AppendFormat("\t\tprivate void OnClick_{0}()", zNewBindObj.tag)
                             .AppendLine()
                             .AppendLine("\t\t{")
                             .AppendLine("\t\t}")
                             .AppendLine();
                            }
                        }
                        else if (_tempType == typeof(UnityEngine.UI.Toggle))
                        {
                            sbEvent
                           .AppendFormat("\t\tprivate void OnToggle_{0}(bool isOn)", zNewBindObj.tag)
                           .AppendLine()
                           .AppendLine("\t\t{")
                           .AppendLine("\t\t}")
                           .AppendLine();
                        }
                        else if (_tempType == typeof(UnityEngine.UI.Slider))
                        {
                            sbEvent
                           .AppendFormat("\t\tprivate void OnSlider_{0}(float process)", zNewBindObj.tag)
                           .AppendLine()
                           .AppendLine("\t\t{")
                           .AppendLine("\t\t}")
                           .AppendLine();

                        }
                    }
                }
            }
            sb.Append("\t\t#endregion");
            sb.AppendLine();


            sbEvent.Append("\t\t#endregion");
            sbEvent.AppendLine();


            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine(sbEvent.ToString());

            //输出日志以便减少代码的
            LogHandler.Info(sb.ToString());

            UnityEngine.GUIUtility.systemCopyBuffer = sb.ToString();

            // Debug.Log("\n\n\n\n\n");

            // Debug.Log(sbEvent.ToString());


            EditorUtility.SetDirty(target);
        }

        private int SortMonoBehaviour(MonoBehaviour mono1, MonoBehaviour mono2)
        {
            return GetTypeWeight(mono2.GetType()) - GetTypeWeight(mono1.GetType());
        }

        private int GetTypeWeight(System.Type type)
        {
            int weight = 0;
            if (type == typeof(UnityEngine.UI.ScrollRect))
                weight = 210;
            else if (type == typeof(UnityEngine.UI.Button))
                weight = 200;
            else if (type == typeof(UnityEngine.UI.Toggle))
                weight = 199;
            else if (type == typeof(UnityEngine.UI.Slider))
                weight = 198;
            else if (type == typeof(UnityEngine.UI.Image))
                weight = 190;
            else if (type == typeof(TMPro.TMP_InputField))
                weight = 185;
            else if (type == typeof(UnityEngine.UI.Text))
                weight = 180;
            else if (type == typeof(TMPro.TextMeshProUGUI))
                weight = 170;
            else if (type == typeof(UnityEngine.UI.HorizontalLayoutGroup))
                weight = 160;
            else if (type == typeof(UnityEngine.UI.VerticalLayoutGroup))
                weight = 150;
            else if (type == typeof(UnityEngine.UI.GridLayoutGroup))
                weight = 140;

            //一些不重要的UI组件
            else if (type == typeof(UnityEngine.UI.ContentSizeFitter))
                weight = 1;
            else if (type == typeof(UnityEngine.UI.LayoutElement))
                weight = 1;
            else if (type == typeof(UnityEngine.UI.Shadow))
                weight = 1;
            else if (type == typeof(UnityEngine.UI.Outline))
                weight = 1;
            else if (type == typeof(Coffee.UIExtensions.UIShiny))
                weight = 1;
            else if (type == typeof(LocalizationForUndeal))
                weight = 1;
            else if (type == typeof(UIButtonScale))
                weight = 1;
            else if (type == typeof(UIGray))
                weight = 50;
            else
                weight = 1000; //自定义类型最高

            return weight;
        }


        static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }


        static bool mEndHorizontal = false;
        static public void BeginContents(bool minimalistic)
        {
            if (!minimalistic)
            {
                mEndHorizontal = true;
                GUILayout.BeginHorizontal();
                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            }
            else
            {
                mEndHorizontal = false;
                EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
                GUILayout.Space(10f);
            }
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }


        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (mEndHorizontal)
            {
                GUILayout.Space(3f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3f);
        }

        private Transform FindPanel()
        {
            UIConfigMono zTarget = (UIConfigMono)target;
            Transform panelTransform = null;
            for (int i = 0; i < zTarget.transform.childCount; i++)
            {
                Transform child = zTarget.transform.GetChild(i);
                if (child.name == "Panel")
                {
                    panelTransform = child;
                    break;
                }
            }
            return panelTransform;
        }

        private void OnValidatePanel()
        {
            if (Application.isPlaying || !isCreatePanel || m_AspectSafeAreaHelper != null) return;

            Transform panelTransform = FindPanel();
            UIConfigMono zTarget = (UIConfigMono)target;
            if (panelTransform != null)
            {
                AspectSafeAreaHelper helper = panelTransform.gameObject.GetComponent<AspectSafeAreaHelper>();
                if (helper == null)
                {
                    m_AspectSafeAreaHelper = panelTransform.gameObject.AddComponent<AspectSafeAreaHelper>();
                    EditorUtility.SetDirty(zTarget);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                GameObject panel = new GameObject("Panel");
                panel.layer = LayerMask.NameToLayer("UI");
                panel.transform.SetParent(zTarget.transform);
                RectTransform rectTransform = panel.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                m_AspectSafeAreaHelper = panel.AddComponent<AspectSafeAreaHelper>();
                EditorUtility.SetDirty(zTarget);
                AssetDatabase.SaveAssets();
            }
        }

    }
}


