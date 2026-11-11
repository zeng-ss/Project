using System.Collections.Generic;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class RoadNavigator
    {
        private Vector3[] _pts;
        private float[] _dists;
        private float _totalLength;

        public float TotalLength => _totalLength;

        // 中心线首尾点：开环道路的传送门位置（车从终点进入、起点出来）
        public Vector3 StartPoint => _pts[0];
        public Vector3 StartDirection => (_pts[0] - _pts[1]).normalized;
        public Vector3 EndPoint => _pts[^1];
        public Vector3 EndDirection => (_pts[^1] - _pts[^2]).normalized;

        public RoadNavigator(List<Vector3> centerline, int cellCount) => Rebuild(centerline, cellCount);

        public void Rebuild(List<Vector3> centerline, int cellCount)
        {
            int n = centerline.Count;
            _pts = new Vector3[n];
            _dists = new float[n];
            _pts[0] = centerline[0];
            _dists[0] = 0f;
            for (int i = 1; i < n; i++)
            {
                _pts[i] = centerline[i];
                _dists[i] = _dists[i - 1] + Vector3.Distance(_pts[i - 1], _pts[i]);
            }
            _totalLength = _dists[n - 1];
            var cellSize = _totalLength / cellCount;
            Debug.Log($"[RoadNav] Rebuild: pts={n} totalLength={_totalLength:F2} cells={cellCount} cellSize={cellSize:F2}");
        }

        private float WrapDistance(float d)
        {
            if (_totalLength <= 0f) return 0f;
            d %= _totalLength;
            if (d < 0f) d += _totalLength;
            return d;
        }

        public float AdvanceDistance(float current, float delta) => WrapDistance(current + delta);

        public Vector3 GetPosition(float d)
        {
            d = WrapDistance(d);
            int i = FindIndex(d);
            if (i >= _pts.Length - 1) return _pts[^1];
            float seg = _dists[i + 1] - _dists[i];
            float t = seg > 0f ? (d - _dists[i]) / seg : 0f;
            return Vector3.Lerp(_pts[i], _pts[i + 1], t);
        }

        public Vector3 GetDirection(float d)
        {
            d = WrapDistance(d);
            int i = FindIndex(d);
            if (i >= _pts.Length - 1) i = _pts.Length - 2;
            return (_pts[i + 1] - _pts[i]).normalized;
        }

        private int FindIndex(float d)
        {
            int lo = 0, hi = _dists.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (_dists[mid] < d) lo = mid + 1;
                else hi = mid;
            }
            return Mathf.Max(0, lo - 1);
        }

        /// <summary>
        /// 用 FrontPoint 算出真正的出口方向，避免 ComputeHolderExit 方向颠倒
        /// 再沿该射线找侧向距离最近的中心线点。
        /// </summary>
        public void ProbeAssign(List<Holder> holders, List<(Vector3 frontCenter, Vector3 exitDir)> exits)
        {
            int count = holders.Count;
            var bestProj = new float[count];
            var bestDist = new float[count];
            var bestPos = new Vector3[count];
            var bestLatVal = new float[count];
            for (int h = 0; h < count; h++) bestProj[h] = float.MaxValue;

            for (int i = 0; i < _pts.Length; i++)
            {
                for (int h = 0; h < count; h++)
                {
                    var ed = exits[h].exitDir;
                    var fc = exits[h].frontCenter;
                    float dx = _pts[i].x - fc.x;
                    float dz = _pts[i].z - fc.z;
                    float proj = dx * ed.x + dz * ed.z;
                    if (proj <= 0f) continue;

                    float lx = dx - proj * ed.x;
                    float lz = dz - proj * ed.z;
                    float lat = lx * lx + lz * lz;
                    if (lat > 4f) continue;

                    if (proj < bestProj[h]) { bestProj[h] = proj; bestDist[h] = _dists[i]; bestPos[h] = _pts[i]; bestLatVal[h] = Mathf.Sqrt(lat); }
                }
            }

            for (int h = 0; h < count; h++)
            {
                holders[h].RoadConnectionDistance = bestDist[h];
                holders[h].RoadEntryPoint = bestPos[h];
                //Debug.Log($"[RoadNav] Holder[{h}]: fc=({exits[h].frontCenter.x:F1},{exits[h].frontCenter.z:F1}) lat={bestLatVal[h]:F2} proj={bestProj[h]:F1} dist={bestDist[h]:F1}");
            }
        }

        public bool CrossedDistance(float prevDist, float curDist, float targetDist)
        {
            float halfLen = _totalLength / 2f;
            float dPrev = prevDist - targetDist;
            float dCur = curDist - targetDist;
            dPrev = ((dPrev + halfLen) % _totalLength + _totalLength) % _totalLength - halfLen;
            dCur = ((dCur + halfLen) % _totalLength + _totalLength) % _totalLength - halfLen;
            return dPrev < 0f && dCur >= 0f;
        }
    }
}
