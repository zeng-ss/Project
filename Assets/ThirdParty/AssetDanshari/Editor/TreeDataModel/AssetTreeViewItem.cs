using UnityEditor.IMGUI.Controls;
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;

namespace AssetDanshari
{
    public class AssetTreeViewItem<T> : TreeViewItem
    {
        public T data { get; private set; }

        public AssetTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            this.data = data;
        }
    }
}