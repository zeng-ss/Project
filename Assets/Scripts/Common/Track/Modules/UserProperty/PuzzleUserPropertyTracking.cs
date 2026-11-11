using CasualGame;
using UnityEngine;
using System.Collections.Generic;

namespace PuzzleTrack
{
    public sealed class PuzzleUserPropertyTracking : PuzzleTrackingModule
    {
        public override string ModuleName => "user_property";

        public void UpdateUserProperties(int level)
        {
            // if (ProxyManager.UserProxy != null && ProxyManager.UserProxy.userModel != null)
            // {
            //     HorstRillSdk.Data.SetSuperProperties("puzzle_level", level.ToString());
            // }
        }
    }
}
