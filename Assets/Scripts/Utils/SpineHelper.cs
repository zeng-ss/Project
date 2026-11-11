//------------------------------------------------------------
// File : SpineHelper.cs
// Email: zhiqiang.yang@kingboat.io
// Desc : spine helper
//------------------------------------------------------------

using Spine;
using Spine.Unity;

namespace CasualGame
{
    public class SpineHelper
    {
        private SkeletonAnimation skel;
        private SkeletonGraphic graphic;
        private Skin rootSkin;
        private SkeletonData skeletonData
        {
            get
            {
                if (skel != null)
                    return skel.skeleton.Data;
                else if (graphic != null)
                {
                    return graphic.SkeletonData;
                }
                return null;
            }
        }

        public SpineHelper(SkeletonAnimation _skel)
        {
            skel = _skel;
        }


        public SpineHelper(SkeletonGraphic _graphic)
        {
            graphic = _graphic;
        }

        public void ResetSkin()
        {
            rootSkin = new Skin("change-base");
        }

        public Skin FindSkin(string skinName)
        {
            if (skeletonData == null) return null;

            return skeletonData.FindSkin(skinName);
        }

        public void AddSkin(string skinName)
        {
            var skin = FindSkin(skinName);

            if (skin != null)
                rootSkin.AddSkin(skin);
        }

        public void RefreshSkin()
        {
            if (skel != null)
            {
                skel.skeleton.SetSkin(rootSkin);
                skel.skeleton.SetSlotsToSetupPose();
            }
            else if (graphic != null)
            {
                graphic.Skeleton.SetSkin(rootSkin);
                graphic.Skeleton.SetSlotsToSetupPose();
            }
        }

        public void ResetSkeleton()
        {
            if (skel != null)
            {
                skel.skeleton.SetToSetupPose();
                skel.AnimationState.ClearTracks();
                skel.skeleton.SetSlotsToSetupPose();
            }
            else if (graphic != null)
            {
                graphic.Skeleton.SetToSetupPose();
                graphic.AnimationState.ClearTracks();
                graphic.Skeleton.SetSlotsToSetupPose();
            }

        }
    }
}