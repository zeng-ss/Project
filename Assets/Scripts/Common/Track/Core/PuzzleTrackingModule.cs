using System.Collections.Generic;

namespace PuzzleTrack
{
    public interface IPuzzleTrackingModule
    {
        string ModuleName { get; }
    }

    public abstract class PuzzleTrackingModule : IPuzzleTrackingModule
    {
        public abstract string ModuleName { get; }
        protected PuzzleLevelSessionContext Session => PuzzleTracking.Session;
        protected PuzzleUserPropertyTracking UserProperties => PuzzleTracking.UserProperties;

        protected void Track(string eventName, Dictionary<string, object> data)
        {
            PuzzleTracking.Track(eventName, data);
        }

        protected void TrackRevenue(string eventName, Dictionary<string, object> data, string currencyType, float payAmount)
        {
            PuzzleTracking.TrackRevenue(eventName, data, currencyType, payAmount);
        }

        protected string Safe(string value)
        {
            return PuzzleTracking.Safe(value);
        }
    }
}
