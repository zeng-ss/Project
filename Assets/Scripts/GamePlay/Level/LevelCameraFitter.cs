using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 挂在 Camera 上：斜俯视正交相机关卡适配，保持机位高度与旋转，
    /// 用「注视点偏移」对齐内容，再按包围盒在相机本地空间投影计算 orthographicSize。
    /// inactive 的 Holder 不参与包围盒计算；ReceiptPrinter 计入包围盒。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class LevelCameraFitter : MonoBehaviour
    {
        [Tooltip("左右各占屏幕宽度的留白比例，例如 0.05=左右各留 5%")]
        [SerializeField] private float paddingLR = 0.05f;
        [SerializeField] private float paddingTop = 0.13f;
        [SerializeField] private float paddingBottom = 0.17f;
        [SerializeField] private float minOrthographicSize = 12f;
        [SerializeField] private float roadHalfWidth = 1f;
        [SerializeField] private float refitDuration = 0.35f;

        private const float HalfCarWidthSpacing = 1f;
        private const float HalfCarDepthSpacing = LevelBuilder.CarDepthSpacing / 2f;

        private Camera _camera;
        private Sequence _seq;
        private readonly Vector3[] _corners = new Vector3[8];

        private void Awake() => _camera = GetComponent<Camera>();

        private void OnDestroy() => _seq?.Kill();

        /// <summary>关卡初始加载：瞬间适配。</summary>
        public void Fit(LevelBuilder.PathInfo pathInfo, IReadOnlyList<Holder> holders,
            ReceiptPrinter receiptPrinter = null)
        {
            _seq?.Kill();
            if (!TryComputeTarget(pathInfo, holders, receiptPrinter, out var targetPos, out var targetOrthoSize))
                return;
            ApplyTarget(targetPos, targetOrthoSize);
        }

        /// <summary>运行时重适配：与 Fit 相同计算，平滑过渡。</summary>
        public void Refit(LevelBuilder.PathInfo pathInfo, IReadOnlyList<Holder> holders,
            ReceiptPrinter receiptPrinter = null)
        {
            if (!TryComputeTarget(pathInfo, holders, receiptPrinter, out var targetPos, out var targetOrthoSize))
                return;

            Vector3 startPos = transform.position;
            float startOrtho = _camera.orthographicSize;

            if (Vector3.SqrMagnitude(startPos - targetPos) < 0.0001f &&
                Mathf.Abs(startOrtho - targetOrthoSize) < 0.001f)
                return;

            _seq?.Kill();
            _seq = DOTween.Sequence().SetTarget(this);
            _seq.Join(transform.DOMove(targetPos, refitDuration).SetEase(Ease.OutCubic));

            if (_camera.orthographic)
            {
                _seq.Join(DOTween.To(
                    () => _camera.orthographicSize,
                    v => _camera.orthographicSize = v,
                    targetOrthoSize,
                    refitDuration).SetEase(Ease.OutCubic));
            }
        }

        private void ApplyTarget(Vector3 targetPos, float targetOrthoSize)
        {
            transform.position = targetPos;
            if (_camera.orthographic)
                _camera.orthographicSize = targetOrthoSize;
        }

        private bool TryComputeTarget(
            LevelBuilder.PathInfo pathInfo,
            IReadOnlyList<Holder> holders,
            ReceiptPrinter receiptPrinter,
            out Vector3 targetPos,
            out float targetOrthoSize)
        {
            targetPos = transform.position;
            targetOrthoSize = _camera != null ? _camera.orthographicSize : minOrthographicSize;

            if (_camera == null) return false;
            if (!TryGetBounds(pathInfo, holders, receiptPrinter, out var bounds)) return false;

            float groundY = LevelBuilder.OrigionPos.y;
            Vector3 targetLookAt = new Vector3(bounds.center.x, groundY, bounds.center.z);

            if (TryGetGroundLookAt(groundY, out var currentLookAt))
            {
                Vector3 offset = transform.position - currentLookAt;
                targetPos = targetLookAt + offset;
            }
            else
            {
                Vector3 camPos = transform.position;
                targetPos = new Vector3(targetLookAt.x, camPos.y, targetLookAt.z);
            }

            if (!_camera.orthographic)
                return true;

            targetOrthoSize = ComputeOrthographicSize(bounds, targetPos);
            // 上下留白不对称时，把内容中心偏到可用区中点（相对屏中心偏移）
            float yUsable = Mathf.Clamp(1f - paddingTop - paddingBottom, 0.01f, 1f);
            float contentCenterV = paddingBottom + yUsable * 0.5f;
            float desiredLocalY = (contentCenterV - 0.5f) * 2f * targetOrthoSize;
            targetPos -= transform.up * desiredLocalY;
            return true;
        }

        /// <summary>在目标机位下投影包围盒，Fit/Refit 必须使用同一套算法。</summary>
        private float ComputeOrthographicSize(Bounds bounds, Vector3 camPos)
        {
            float maxAbsX = 0f;
            float maxAbsY = 0f;
            GetBoundsCorners(bounds, _corners);
            Quaternion invRot = Quaternion.Inverse(transform.rotation);
            for (int i = 0; i < 8; i++)
            {
                Vector3 local = invRot * (_corners[i] - camPos);
                maxAbsX = Mathf.Max(maxAbsX, Mathf.Abs(local.x));
                maxAbsY = Mathf.Max(maxAbsY, Mathf.Abs(local.y));
            }

            float aspect = Mathf.Max(_camera.aspect, 0.01f);
            // 左/右各 paddingX；上 paddingTop、下 paddingBottom（整屏比例）。取 Max 两轴都满足
            float xUsable = Mathf.Clamp(1f - 2f * paddingLR, 0.01f, 1f);
            float yUsable = Mathf.Clamp(1f - paddingTop - paddingBottom, 0.01f, 1f);
            float sizeForHeight = maxAbsY / yUsable;
            float sizeForWidth = maxAbsX / (aspect * xUsable);
            return Mathf.Max(minOrthographicSize, sizeForHeight, sizeForWidth);
        }

        private bool TryGetGroundLookAt(float groundY, out Vector3 lookAt)
        {
            lookAt = default;
            var plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
            var ray = new Ray(transform.position, transform.forward);
            if (!plane.Raycast(ray, out float enter) || enter <= 0f)
                return false;

            lookAt = ray.GetPoint(enter);
            return true;
        }

        private bool TryGetBounds(
            LevelBuilder.PathInfo pathInfo,
            IReadOnlyList<Holder> holders,
            ReceiptPrinter receiptPrinter,
            out Bounds bounds)
        {
            bounds = default;
            bool hasPoint = false;

            if (pathInfo.Nodes != null)
            {
                foreach (var node in pathInfo.Nodes)
                {
                    var p = node.WorldPos;
                    Encapsulate(ref bounds, ref hasPoint, p + new Vector3(roadHalfWidth, 0f, roadHalfWidth));
                    Encapsulate(ref bounds, ref hasPoint, p - new Vector3(roadHalfWidth, 0f, roadHalfWidth));
                }
            }

            if (holders != null)
            {
                foreach (var holder in holders)
                {
                    if (holder == null || !holder.gameObject.activeInHierarchy) continue;
                    EncapsulateHolder(ref bounds, ref hasPoint, holder.Info);
                }
            }

            EncapsulateReceiptPrinter(ref bounds, ref hasPoint, receiptPrinter);
            return hasPoint;
        }

        private static void EncapsulateReceiptPrinter(ref Bounds bounds, ref bool hasPoint,
            ReceiptPrinter printer)
        {
            if (printer == null || !printer.gameObject.activeInHierarchy) return;

            var renderers = printer.GetComponentsInChildren<Renderer>();
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!hasPoint)
                {
                    bounds = r.bounds;
                    hasPoint = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }

                any = true;
            }

            if (!any)
                Encapsulate(ref bounds, ref hasPoint, printer.transform.position);
        }

        private static void GetBoundsCorners(Bounds bounds, Vector3[] corners)
        {
            Vector3 c = bounds.center;
            Vector3 e = bounds.extents;
            corners[0] = c + new Vector3(-e.x, -e.y, -e.z);
            corners[1] = c + new Vector3(-e.x, -e.y, e.z);
            corners[2] = c + new Vector3(-e.x, e.y, -e.z);
            corners[3] = c + new Vector3(-e.x, e.y, e.z);
            corners[4] = c + new Vector3(e.x, -e.y, -e.z);
            corners[5] = c + new Vector3(e.x, -e.y, e.z);
            corners[6] = c + new Vector3(e.x, e.y, -e.z);
            corners[7] = c + new Vector3(e.x, e.y, e.z);
        }

        private static void EncapsulateHolder(ref Bounds bounds, ref bool hasPoint,
            LevelBuilder.HolderInfo holder)
        {
            float halfPerp = holder.Width * HalfCarWidthSpacing;
            float halfDepth = holder.Height * HalfCarDepthSpacing;
            Quaternion rot = Quaternion.Euler(0f, GetHolderYaw(holder), 0f);
            Vector3 anchor = holder.AnchorWorldPos;

            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 world = anchor + rot * new Vector3(sx * halfPerp, 0f, sz * halfDepth);
                    Encapsulate(ref bounds, ref hasPoint, world + Vector3.up * 0.5f);
                    Encapsulate(ref bounds, ref hasPoint, world - Vector3.up * 0.5f);
                }
        }

        private static float GetHolderYaw(LevelBuilder.HolderInfo holder) =>
            holder.DirectionValue ?? holder.Direction switch
            {
                LevelBuilder.HolderDirection.Right => 90f,
                LevelBuilder.HolderDirection.Down => 180f,
                LevelBuilder.HolderDirection.Left => 270f,
                _ => 0f
            };

        private static void Encapsulate(ref Bounds bounds, ref bool hasPoint, Vector3 point)
        {
            if (!hasPoint)
            {
                bounds = new Bounds(point, Vector3.zero);
                hasPoint = true;
            }
            else
            {
                bounds.Encapsulate(point);
            }
        }
    }
}
