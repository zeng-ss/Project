using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public class UIDepth : MonoBehaviour
    {
        public int order;
        public bool isUI = true;
        public bool raycast = false;

        private bool isRefresh;

        public bool IsRefreshed { get { return isRefresh; } }

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            Canvas parentCanvas = CUtility.Mono.GetParentFirstComponent<Canvas>(gameObject);
            int baseOrder = order;
            if (parentCanvas)
            {
                baseOrder = parentCanvas.sortingOrder + order;
            }

            if (isUI)
            {
                Canvas canvas = GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder = baseOrder;

                if (raycast)
                {
                    GraphicRaycaster canvasRaycast = GetComponent<GraphicRaycaster>();
                    if (canvasRaycast == null)
                    {
                        canvasRaycast = gameObject.AddComponent<GraphicRaycaster>();
                    }
                }

            }
            else
            {
                Renderer[] renders = GetComponentsInChildren<Renderer>(true);

                foreach (Renderer render in renders)
                {
                    render.sortingOrder = baseOrder;
                }
            }

            isRefresh = true;
        }
    }
}