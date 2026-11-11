using System;
using System.Collections.Generic;
using Custom.Activity;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public class HomePanel : PanelBase
    {
        public override ActivityType ActivityType => ActivityType.HomePanel;

        public override RectTransform PanelRectTransform => null;

        public override void InitPanel(AtlasLoader loader = null)
        {
        }

        public override void RefreshPanel()
        {
        }

        public override void ReleasePanel()
        {
        }
    }
}