using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityGameFramework.Editor.ResourceTools
{
    [CustomEditor(typeof(Group))]
    public class GroupEditor : UnityEditor.Editor, IDependenciesEditor
    {
        [SerializeField] private TreeViewState treeViewState;
        [SerializeField] private MultiColumnHeaderState multiColumnHeaderState;
        [SerializeField] private TreeViewState dependenciesTreeViewState;

        private BundleMode _bundleMode;
        private SearchField _searchField;
        private DependenciesTreeView dependenciesTreeView;
        private GroupAssetTreeView treeView;

        public void ReloadDependencies(params string[] assetPaths)
        {
            if (dependenciesTreeView == null)
            {
                return;
            }

            dependenciesTreeView.assetPaths.Clear();
            dependenciesTreeView.assetPaths.AddRange(assetPaths);
            dependenciesTreeView.Reload();
            dependenciesTreeView.ExpandAll();
        }

        private void OnEnable()
        {
            // Settings.GetDefaultSettings().Initialize();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("分组建议：" +
                                    "\n1.同一时刻同时使用的资源打包到一起，同一个图集的贴图打包到一起；" +
                                    "\n2.带依赖的和不带依赖的分开打包，易变的和不易变的分开打包；" +
                                    "\n3.打包粒度主要服务于按需加载，如果还有疑问请在对接群留言。", MessageType.Info);
            if (dependenciesTreeViewState == null)
            {
                dependenciesTreeViewState = new TreeViewState();
            }

            if (dependenciesTreeView == null)
            {
                dependenciesTreeView = new DependenciesTreeView(dependenciesTreeViewState);
            }


            if (treeViewState == null)
            {
                treeViewState = new TreeViewState();
                var headerState =
                    GroupAssetTreeView.CreateDefaultMultiColumnHeaderState(); // multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, headerState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, headerState);
                }

                multiColumnHeaderState = headerState;
            }
            var group = target as Group;
            if (group == null)
            {
                return;
            }

            if (treeView == null)
            {
                treeView = new GroupAssetTreeView(treeViewState, multiColumnHeaderState, this);
                Reload(group);
            }

            if (_searchField == null)
            {
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            }

            if (GUILayout.Button("Refresh"))
            {
                // Settings.GetDefaultSettings().Initialize();
                Reload(group);
            }


            if (GUILayout.Button("Div dep"))
            {
                DivDep(group);
            }


            if (GUILayout.Button("Print"))
            {
                PrintDeps();
            }

            treeView.searchString = _searchField.OnGUI(treeView.searchString);
            treeView.OnGUI(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(300)));
            dependenciesTreeView.OnGUI(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(300)));

            if (_bundleMode != group.bundleMode)
            {
                treeView?.Repaint();
            }

            _bundleMode = group.bundleMode;

        }

        private void Reload(Group group)
        {
            if (treeView == null) return;
            treeView.assets.Clear();
            Group.CollectAssets(group, (path, entry) =>
            {
                var asset = group.CreateAsset(path, entry);
                treeView.assets.Add(asset);
            });
            treeView.Reload();
        }


        Dictionary<GroupAsset, List<string>> pathTAsset = new Dictionary<GroupAsset, List<string>>();
        Dictionary<string, int> assetDep = new Dictionary<string, int>();//资源引用计数器

        private void DivDep(Group group)
        {
            pathTAsset.Clear();
            assetDep.Clear();

            Group.CollectAssets(group, (path, entry) =>
           {
               var asset = group.CreateAsset(path, entry);
               //    pathTAsset.Add( asset);

               var deps = Group.GetDep(asset.path);
               for (int i = 0; i < deps.Count; i++)
               {
                   string _path = deps[i];
                   if (assetDep.ContainsKey(_path)) { assetDep[_path]++; }
                   else
                   {
                       assetDep.Add(_path, 1);
                   }
               }
           });
        }

        private void PrintDeps()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in assetDep)
            {
                stringBuilder.AppendLine(item.Key + " -> " + item.Value);
            }
            Debug.Log(stringBuilder.ToString());

            System.IO.File.WriteAllText(Application.persistentDataPath + "/dep.txt", stringBuilder.ToString());
        }



    }
}