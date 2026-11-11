using System.Collections.Generic;

namespace PuzzleTrack
{
    public sealed class PuzzleUITracking : PuzzleTrackingModule
    {
        public override string ModuleName => "ui";

        public void ClickButton(string buttonName, string placement = "")
        {
            Track(PuzzleTrackingEvent.UITrack, new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = "click",
                ["guide_name"] = Safe(buttonName),
                ["placement"] = Safe(placement)
            });
        }
    }
}
