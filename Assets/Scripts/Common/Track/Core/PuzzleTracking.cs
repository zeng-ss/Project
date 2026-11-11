using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PuzzleTrack
{
    /// <summary>
    /// Shared entry for puzzle-game tracking. Concrete event APIs live in module classes.
    /// </summary>
    public static class PuzzleTracking
    {
        public const string DefaultMode = "default";
        public const string DefaultPlacement = "ingame";

        private static readonly PuzzleLevelSessionContext s_Session = new PuzzleLevelSessionContext();

        static PuzzleTracking()
        {
            Level = new PuzzleLevelTracking();
            Currency = new PuzzleCurrencyTracking();
            Booster = new PuzzleBoosterTracking();
            Ads = new PuzzleAdsTracking();
            Attribution = new PuzzleAttributionTracking();
            Iap = new PuzzleIapTracking();
            SystemEvents = new PuzzleSystemTracking();
            UI = new PuzzleUITracking();
            UserProperties = new PuzzleUserPropertyTracking();
        }

        public static PuzzleLevelSessionContext Session => s_Session;
        public static PuzzleLevelTracking Level { get; }
        public static PuzzleCurrencyTracking Currency { get; }
        public static PuzzleBoosterTracking Booster { get; }
        public static PuzzleAdsTracking Ads { get; }
        public static PuzzleAttributionTracking Attribution { get; }
        public static PuzzleIapTracking Iap { get; }
        public static PuzzleSystemTracking SystemEvents { get; }
        public static PuzzleUITracking UI { get; }
        public static PuzzleUserPropertyTracking UserProperties { get; }

        internal static void Track(string eventName, Dictionary<string, object> data)
        {
            try
            {
                if (data == null)
                    data = new Dictionary<string, object>();

#if UNITY_EDITOR
                WriteTrackToFile(eventName, data);
#endif
                HorstRillSdk.Data.TrackEvent(eventName, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PuzzleTracking] {eventName} failed: {e.Message}");
            }
        }

#if UNITY_EDITOR
        private static string EditorTrackFilePath =>
            Path.Combine(Application.persistentDataPath, "Track.txt");

        private static void WriteTrackToFile(string eventName, Dictionary<string, object> data)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{eventName}\t{FormatTrackData(data)}{Environment.NewLine}";
            File.AppendAllText(EditorTrackFilePath, line, Encoding.UTF8);
        }

        private static string FormatTrackData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                return "{}";

            var parts = new List<string>(data.Count);
            foreach (var pair in data)
                parts.Add($"{pair.Key}={pair.Value}");

            return "{" + string.Join(", ", parts) + "}";
        }
#endif

        internal static void TrackRevenue(string eventName, Dictionary<string, object> data, string currencyType, float payAmount)
        {
            try
            {
                if (data == null)
                    data = new Dictionary<string, object>();

                HorstRillSdk.Data.TrackRevenueEvent(eventName, data, currencyType, payAmount);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PuzzleTracking] {eventName} revenue failed: {e.Message}");
            }
        }

        internal static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }
    }
}
