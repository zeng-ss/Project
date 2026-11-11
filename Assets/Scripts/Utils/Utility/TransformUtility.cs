//------------------------------------------------------------
// File : TransformUtility.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

namespace CasualGame
{
    using UnityEngine;

    public class TransformUtility
    {
        /// <summary>
        /// 场景相机
        /// </summary>
        public static Camera ScenceCamera
        {
            get
            {
                if (_scenceCamera == null)
                {
                    _scenceCamera = Camera.main;
                }

                return _scenceCamera;
            }

            set => _scenceCamera = value;
        }

        private static Camera _scenceCamera;


        public static Camera UGUICamera => GameEntry.UI.MainCanvas.worldCamera;

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Vector3 worldPoint)
        {
            return RectTransformUtility.WorldToScreenPoint(ScenceCamera, worldPoint);
        }

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Camera cam, Vector3 worldPoint)
        {
            return RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screenPoint, out Vector3 worldPoint)
        {
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, UGUICamera, out worldPoint);
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
        {
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out worldPoint);
        }

        /// <summary>
        /// 屏幕坐标转某个RectTransform下的localPosition坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, UGUICamera, out localPoint);
        }

        /// <summary>
        /// 屏幕坐标转某个RectTransform下的localPosition坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
        }

        //同一个Canvas下的坐标转换。
        public static Vector2 WorldToScreenPoints(Camera camera, RectTransform otherRect, Vector3 world)
        {
            //把世界转屏幕
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, world);
            Vector2 localPoint;
            //屏幕转局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(otherRect, screenPoint, camera, out localPoint);
            return localPoint;
        }


        public static Vector2 WorldToScreenPointsForOtherCamera(Camera camera, RectTransform otherRect, Camera otherCamera, Vector3 world)
        {
            //把世界转屏幕
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, world);
            Vector2 localPoint;
            //屏幕转局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(otherRect, screenPoint, otherCamera, out localPoint);
            return localPoint;
        }

        /// <summary>
        /// 同一个Canvas下的坐标转换。
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoints(Canvas canvas, Vector3 world)
        {
            return WorldToScreenPoints(canvas.worldCamera, canvas.GetComponent<RectTransform>(), world);
        }
    }
}


