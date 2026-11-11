using System.Collections.Generic;
using UnityEngine;

namespace PuzzleTrack
{
    public sealed class PuzzleBoosterTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "booster";

        public void BoosterIn(string boosterName, int amount, string placement)
        {
            Track(PuzzleTrackingEvent.BoosterTrack, new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = "in",
                ["guide_name"] = Safe(boosterName),
                ["value"] = Mathf.Abs(amount),
                ["placement"] = Safe(placement)
            });
        }

        public void UseBooster(string boosterName, string placement = PuzzleTracking.DefaultPlacement)
        {
            Session.MarkUseBooster();

            Track(PuzzleTrackingEvent.BoosterTrack, new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = "out",
                ["guide_name"] = Safe(boosterName),
                ["placement"] = Safe(placement)
            });
        }
    }
}
