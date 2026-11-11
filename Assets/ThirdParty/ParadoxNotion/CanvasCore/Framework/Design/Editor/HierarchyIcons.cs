#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor
{

    [UnityEditor.InitializeOnLoad]
    static class HierarchyIcons
    {
        static HierarchyIcons() {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= ShowIcon;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += ShowIcon;
        }

        static void ShowIcon(EntityId ID, Rect r) {
            if ( !Prefs.showHierarchyIcons ) {
                return;
            }
            var go = EditorUtility.EntityIdToObject(ID) as GameObject;
            if ( go == null ) return;
            var owner = go.GetComponent<GraphOwner>();
            if ( owner == null ) return;
            r.xMin = r.xMax - 16;
            GUI.DrawTexture(r, StyleSheet.canvasIcon);
        }
    }
}

#endif