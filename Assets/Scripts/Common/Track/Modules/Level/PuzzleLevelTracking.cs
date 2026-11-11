using System;
using System.Collections.Generic;
// using CasualGame.ShipJam;

namespace PuzzleTrack
{
    public sealed class PuzzleLevelTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "level";

        public void StartLevel(int level, string mode = "")
        {
            if (Session.IsAttemptActive)
            {
                LoseLevel("restart");
            }

            Session.Start(level, mode);
            var dict = new Dictionary<string, object>
            {
                ["action_type"] = "start",
                ["action"] = "start",
                ["levelid"] = level,
                ["mode"] = Session.Mode,
            };
            TrackLevel(PuzzleTrackingEvent.LevelTrack, dict);
        }

        public void MarkAds()
        {
            Session.MarkAds();
        }

        public void UpdateLevelProgress(int finishCount, int totalCount)
        {
            Session.SetProgress(finishCount, totalCount);
        }

        public void RecordClickPosition(string levelType)
        {
            var dict = new Dictionary<string, object>
            {
                ["level_type"] = Safe(levelType)
            };
            TrackLevel(PuzzleTrackingEvent.LevelRecord, dict);
        }

        public void WinLevel()
        {
            if (!Session.IsAttemptActive)
            {
                return;
            }

            Session.StopTimer();

            var dict = new Dictionary<string, object>
            {
                ["action_type"] = "end",
                ["action"] = "end",
                ["result"] = "win",
                ["levelid"] = Session.Level,
                ["mode"] = Session.Mode,
                ["duration"] = Session.Duration,
                ["progress_ratio"] = 100,
                ["use_boosters"] = Session.IsUseBooster ? 1 : 0,
                ["adsnum"] = Session.IsViewAds ? 1 : 0
            };
            TrackLevel(PuzzleTrackingEvent.LevelTrack, dict);
        }

        public void LoseLevel(string loseReason = "real_fail")
        {
            if (!Session.IsAttemptActive)
            {
                return;
            }

            Session.StopTimer();

            var dict = new Dictionary<string, object>
            {
                ["action_type"] = "end",
                ["action"] = "end",
                ["result"] = "lose",
                ["levelid"] = Session.Level,
                ["mode"] = Session.Mode,
                ["duration"] = Session.Duration,
                ["progress_ratio"] = Session.Progress,
                ["fail_reason"] = Safe(loseReason),
                ["use_boosters"] = Session.IsUseBooster ? 1 : 0,
                ["adsnum"] = Session.IsViewAds ? 1 : 0
            };
            TrackLevel(PuzzleTrackingEvent.LevelTrack, dict);
        }

        public void UseHelper(string helperName, string changeType)
        {
            Session.MarkUseBooster();

            var dict = new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = "helper",
                ["levelid"] = Session.Level,
                ["mode"] = Session.Mode,
                ["duration"] = Session.Duration,
                ["progress_ratio"] = Session.Progress,
                ["item_gain"] = Safe(helperName),
                ["change_type"] = Safe(changeType)
            };
            TrackLevel(PuzzleTrackingEvent.LevelTrack, dict);
        }

        public void Revive(string reviveReason, string reviveType)
        {
            var dict = new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = "revive",
                ["levelid"] = Session.Level,
                ["mode"] = Session.Mode,
                ["duration"] = Session.Duration,
                ["progress_ratio"] = Session.Progress,
                ["change_name"] = Safe(reviveReason),
                ["change_type"] = Safe(reviveType)
            };
            TrackLevel(PuzzleTrackingEvent.LevelTrack, dict);
        }

        private void TrackLevel(string eventName, Dictionary<string, object> data)
        {
            AddCommonParams(data);
            Track(eventName, data);
        }

        private void AddCommonParams(Dictionary<string, object> data)
        {
            if (data == null) return;

            // data["custom_data"] = $"{ABLevelCurveHelper.AdjustLevelCurveKey}:{Session.LevelExperimentValue}";
        }
    }
}
