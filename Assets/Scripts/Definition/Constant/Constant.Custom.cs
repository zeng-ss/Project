using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public static partial class Constant
    {
        public static class Setting
        {
            public const string Language = "Setting.Language2";
            public const string UserProxy = "Proxy.UserProxy";
            public const string SortProxy = "Proxy.SortProxy";
        }

        public static class Scene
        {
            public static string Home = "Home";
            public static string MyGame = "MyGame";
        }

        public static class Event
        {
            public const int LanguageChangeSuccess = 1; //多语言切换成功
            public const int RefreshItem = 2; //刷新体力
            public const int UpdateBtnActive = 3;
            public const int UpdateBtnRedPoint = 4;
            public const int ChangeOrthographicSize = 5;
            public const int DecorationRemoved = 6;
            public const int UpdateRoomDecorationCount = 7;
            public const int IAPPriceRefresh = 8;
            public const int GuideEventExecute = 9;
            public const int GuideHammerUnlock = 10;
            public const int UpdateAvatar = 11;
            public const int ChangePetResource = 30;
            public const int ChangePetTask = 31;
            public const int PetTaskFinished = 32;

            public const int OnGameFail = 1001;
            public const int OnGameRestart = 1003;
            public const int OnTempHoleFull = 2001;
            public const int OnBoxComplete = 2002;
            public const int OnGreenScreen = 2003;
        }

        public static class Layer
        {
            public const string DefaultLayerName = "Default";
            public static int DefaultLayerId => LayerMask.NameToLayer(DefaultLayerName);
            public const string UILayerName = "UI";
            public static int UILayerId => LayerMask.NameToLayer(UILayerName);
            public const string TubeLayerName = "Tube";
            public static int TubeLayerId => LayerMask.NameToLayer(TubeLayerName);
            public const string HighLayerName = "High";
            public static int HighLayerId => LayerMask.NameToLayer(HighLayerName);

            public const string HolderLayerName = "Holder";
            public static int HolderLayerId => LayerMask.NameToLayer(HolderLayerName);

            public const string BgLayerName = "Bg";
            public static int BgLayerId => LayerMask.NameToLayer(BgLayerName);
            public const string BuildingLayerName = "Building";
            public static int BuildingLayerId => LayerMask.NameToLayer(BuildingLayerName);
            public const string BuildingDragLayerName = "BuildingDrag";
            public static int BuildingDragLayerId => LayerMask.NameToLayer(BuildingDragLayerName);


            public const string ObstacleLayerName = "Obstacle";
            public static int ObstacleLayerId => LayerMask.NameToLayer(ObstacleLayerName);

            public const string ScrewName = "Screw";
            public static int ScrewLayerId => LayerMask.NameToLayer(ScrewName);

            public const string StaticObstacleName = "StaticObstacle";
            public static int StaticObstacleId => LayerMask.NameToLayer(StaticObstacleName);

            public const string DynamicObstacleName = "DynamicObstacle";
            public static int DynamicObstacleId => LayerMask.NameToLayer(DynamicObstacleName);

            public const string WordUILayerName = "WordUI";
            public static int WordUILayerId => LayerMask.NameToLayer(WordUILayerName);

            public const string TopUILayerName = "TopLayer";
            public static int TopUILayerId => LayerMask.NameToLayer(TopUILayerName);
        }

        public class AtlasId
        {
            public static readonly string Home = "Home";
            public static readonly string General = "General";
            public static readonly string Setting = "Setting";
            public static readonly string Item = "Item";
            public static readonly string LuckySpin = "LuckySpin";
            public static readonly string Treasure = "Treasure";
            public static readonly string ScoreGuide = "ScoreGuide";
            public static readonly string RoomSprites = "RoomSprites";
            public static readonly string Screw = "Screw";
            public static readonly string Avatar = "Avator";
            public static readonly string Frame = "Frame";
            public static readonly string CoffeeColor = "CoffeeColor";
        }
    }
}