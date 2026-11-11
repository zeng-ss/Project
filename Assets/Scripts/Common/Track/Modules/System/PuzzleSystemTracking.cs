using System.Collections.Generic;

namespace PuzzleTrack
{
    public sealed class PuzzleSystemTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "system";

        public void UnityException(string condition, string stackTrace)
        {
            Track(PuzzleTrackingEvent.UnityException, CreateConditionData(condition, stackTrace));
        }

        public void UnityError(string condition, string stackTrace)
        {
            Track(PuzzleTrackingEvent.UnityError, CreateConditionData(condition, stackTrace));
        }

        public void UnityTryCache(string condition, string stackTrace)
        {
            Track(PuzzleTrackingEvent.UnityTryCache, CreateConditionData(condition, stackTrace));
        }

        public void NotificationOpen(string content, int id)
        {
            Track(PuzzleTrackingEvent.NotificationOpen, new Dictionary<string, object>
            {
                ["content"] = Safe(content),
                ["id"] = id
            });
        }

        private Dictionary<string, object> CreateConditionData(string condition, string stackTrace)
        {
            return new Dictionary<string, object>
            {
                ["condition"] = Safe(condition),
                ["stack_trace"] = Safe(stackTrace)
            };
        }
    }
}
