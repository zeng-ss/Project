using System.Collections.Generic;
using UnityEngine;

namespace PuzzleTrack
{
    public sealed class PuzzleCurrencyTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "currency";

        public void HardCurrencyChange(int amount, string currency, string placement)
        {
            Track(PuzzleTrackingEvent.HardCurrency, CreateCurrencyData(amount, currency, placement));
        }

        public void SoftCurrencyChange(int amount, string currency, string placement)
        {
            Track(PuzzleTrackingEvent.SoftCurrency, CreateCurrencyData(amount, currency, placement));
        }

        private Dictionary<string, object> CreateCurrencyData(int amount, string currency, string placement)
        {
            return new Dictionary<string, object>
            {
                ["action_type"] = "action",
                ["action"] = amount >= 0 ? "in" : "out",
                ["value"] = Mathf.Abs(amount),
                ["currency"] = Safe(currency),
                ["placement"] = Safe(placement)
            };
        }
    }
}
