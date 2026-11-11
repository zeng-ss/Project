using System.Collections.Generic;
using CasualGame;

namespace PuzzleTrack
{
    public sealed class PuzzleAdsTracking : PuzzleTrackingModule
    {
        private static readonly HashSet<int> s_ApUaAdsTriggerCounts = new HashSet<int>
        {
            10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
            110, 120, 130, 140, 150, 160, 170, 180, 190, 200,
            300, 400, 500, 600, 700
        };

        public override string ModuleName => "ads";

        public void AdsOpportunityMissing(string adType, string placement, string reason = "not_ready")
        {
            Track(PuzzleTrackingEvent.AdOpportunityMissing, new Dictionary<string, object>
            {
                ["ad_type"] = Safe(adType),
                ["placement"] = Safe(placement),
                ["reason"] = Safe(reason),
                ["levelid"] = ProxyManager.UserProxy.GameLevel,
                ["mode"] = Session.Mode
            });
        }

        public void ApUaAds(int totalAd)
        {
            if (!s_ApUaAdsTriggerCounts.Contains(totalAd))
                return;

            Track(PuzzleTrackingEvent.ApUaAdsPrefix + totalAd, new Dictionary<string, object>());
        }

        public void NewPuzzleAdsImpression(string mode)
        {
            int level = Session.Level;
            Track(PuzzleTrackingEvent.NewPuzzleAdsImpression, new Dictionary<string, object>
            {
                ["levelid"] = level,
                ["adformat"] = mode
            });
        }
    }
}
