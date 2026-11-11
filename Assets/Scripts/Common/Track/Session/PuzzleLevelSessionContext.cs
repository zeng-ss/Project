using UnityEngine;
// using CasualGame.ShipJam;

namespace PuzzleTrack
{
    public sealed class PuzzleLevelSessionContext
    {
        public int Level { get; private set; }
        public string Mode { get; private set; } = PuzzleTracking.DefaultMode;
        public int Progress { get; private set; }
        public bool IsUseBooster { get; private set; }
        public bool IsViewAds { get; private set; }
        public int LevelExperimentValue { get; private set; }
        public int Duration => m_TimerRunning ? ComputeActiveSeconds() : m_Duration;
        public bool IsAttemptActive => m_TimerRunning;

        private float m_AccumulatedActiveTime;
        private float m_SegmentStartTime = -1f;
        private int m_PauseRefCount;
        private int m_Duration;
        private bool m_TimerRunning;

        public void Start(int level, string mode)
        {
            Level = level;
            Mode = string.IsNullOrEmpty(mode) ? PuzzleTracking.DefaultMode : mode;
            Progress = 0;
            IsUseBooster = false;
            IsViewAds = false;
            m_AccumulatedActiveTime = 0f;
            m_SegmentStartTime = Time.unscaledTime;
            m_PauseRefCount = 0;
            m_Duration = 0;
            m_TimerRunning = true;
            LevelExperimentValue = 1;
        }

        public void PauseTimer()
        {
            if (!m_TimerRunning)
                return;

            if (m_PauseRefCount == 0)
                FlushSegment();

            m_PauseRefCount++;
        }

        public void ResumeTimer()
        {
            if (!m_TimerRunning || m_PauseRefCount == 0)
                return;

            m_PauseRefCount--;
            if (m_PauseRefCount == 0)
                m_SegmentStartTime = Time.unscaledTime;
        }

        internal void StopTimer()
        {
            if (!m_TimerRunning)
                return;

            m_Duration = ComputeActiveSeconds();
            m_TimerRunning = false;
            m_SegmentStartTime = -1f;
            m_PauseRefCount = 0;
        }

        public void MarkAds()
        {
            IsViewAds = true;
        }

        public void SetProgress(int finishCount, int totalCount)
        {
            Progress = totalCount > 0
                ? Mathf.Clamp(Mathf.RoundToInt((float)finishCount / totalCount * 100), 0, 100)
                : 0;
        }

        public void MarkUseBooster()
        {
            IsUseBooster = true;
        }

        private void FlushSegment()
        {
            if (m_SegmentStartTime < 0f)
                return;

            m_AccumulatedActiveTime += Time.unscaledTime - m_SegmentStartTime;
            m_SegmentStartTime = -1f;
        }

        private int ComputeActiveSeconds()
        {
            float active = m_AccumulatedActiveTime;
            if (m_PauseRefCount == 0 && m_SegmentStartTime >= 0f)
                active += Time.unscaledTime - m_SegmentStartTime;

            return Mathf.Max(0, Mathf.RoundToInt(active));
        }
    }
}
