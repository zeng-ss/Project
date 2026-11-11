using System.Collections.Generic;

namespace PuzzleTrack
{
    public sealed class PuzzleAttributionTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "attribution";

        public void AfConversionData(Dictionary<string, object> data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            AddConversionValue(dict, data, "af_status", "af_cv_status");
            AddConversionValue(dict, data, "media_source", "af_cv_media_source");
            AddConversionValue(dict, data, "is_first_launch", "af_cv_is_first_launch");
            AddConversionValue(dict, data, "campaign", "af_cv_campaign");
            AddConversionValue(dict, data, "campaign_id", "af_cv_campaign_id");

            Track(PuzzleTrackingEvent.AfConversionData, dict);
        }

        private static void AddConversionValue(
            Dictionary<string, object> dict,
            Dictionary<string, object> source,
            string sourceKey,
            string trackKey)
        {
            if (source == null || !source.ContainsKey(sourceKey))
                return;

            dict[trackKey] = source[sourceKey];
        }
    }
}
