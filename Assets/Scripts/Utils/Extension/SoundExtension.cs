using System;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.Sound;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public static class SoundExtension
    {
        private const float FadeVolumeDuration = 1f;
        private static int? s_MusicSerialId = null;

        public static int? PlayMusic(this SoundComponent soundComponent, int musicId, object userData = null)
        {
            soundComponent.StopMusic();

            IDataTable<DRSound> dtUISound = GameEntry.DataTable.GetDataTable<DRSound>();
            DRSound drUISound = dtUISound.GetDataRow(musicId);
            if (drUISound == null)
            {
                Log.Warning("Can not load UI sound '{0}' from data table.", musicId.ToString());
                return null;
            }

            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById(drUISound.AssetId);

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 64;
            playSoundParams.Loop = drUISound.Loop;
            playSoundParams.VolumeInSoundGroup = drUISound.Volume;
            playSoundParams.FadeInSeconds = FadeVolumeDuration;
            playSoundParams.SpatialBlend = drUISound.SpatialBlend;
            s_MusicSerialId = soundComponent.PlaySound(assetPath, drUISound.SoundGroup,
                Constant.AssetPriority.MusicAsset, playSoundParams, null, userData);
            return s_MusicSerialId;
        }

        public static void StopMusic(this SoundComponent soundComponent)
        {
            if (!s_MusicSerialId.HasValue)
            {
                return;
            }

            soundComponent.StopSound(s_MusicSerialId.Value, FadeVolumeDuration);
            s_MusicSerialId = null;
        }

        public static int? PlaySound(this SoundComponent soundComponent, int soundId)
        {
            IDataTable<DRSound> dtUISound = GameEntry.DataTable.GetDataTable<DRSound>();
            if (dtUISound == null) return null;

            DRSound drUISound = dtUISound.GetDataRow(soundId);
            if (drUISound == null)
            {
                Log.Warning("Can not load UI sound '{0}' from data table.", soundId.ToString());
                return null;
            }

            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById(drUISound.AssetId);

            if (string.IsNullOrEmpty(assetPath))
            {
                return -1;
            }

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = drUISound.Priority;
            playSoundParams.Loop = drUISound.Loop;
            playSoundParams.VolumeInSoundGroup = drUISound.Volume;
            playSoundParams.SpatialBlend = drUISound.SpatialBlend;
            playSoundParams.Time = drUISound.Time;
            playSoundParams.MaxDistance = drUISound.MaxDistance;
            playSoundParams.DopplerLevel = drUISound.DopplerLevel;
            playSoundParams.Pitch = drUISound.Pitch;

            return soundComponent.PlaySound(assetPath, drUISound.SoundGroup,
                Constant.AssetPriority.SoundAsset, playSoundParams, null);
        }

        public static int PlayDubbing(this SoundComponent soundComponent, int assetID)
        {
            return PlaySoundByAssetID(soundComponent, assetID, soundGroup: "Dubbing", volume: 1f);
        }

        public static int PlaySoundByAssetID(this SoundComponent soundComponent, int assetID,
            string soundGroup = "Sound", float spatialBlend = 0, float time = 0, float volume = 0.5f, int priority = 0,
            float maxDistance = 100, float dopplerLevel = 1, float pitch = 1, bool loop = false)
        {
            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById(assetID);

            if (string.IsNullOrEmpty(assetPath))
            {
                return -1;
            }

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.SpatialBlend = spatialBlend;
            playSoundParams.Time = time;
            playSoundParams.VolumeInSoundGroup = volume;
            playSoundParams.Priority = priority;
            playSoundParams.MaxDistance = maxDistance;
            playSoundParams.DopplerLevel = dopplerLevel;
            playSoundParams.Pitch = pitch;
            playSoundParams.Loop = loop;

            return soundComponent.PlaySound(assetPath, soundGroup, Constant.AssetPriority.SoundAsset,
                playSoundParams, null);
        }

        public static bool IsMuted(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return true;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return true;
            }

            return soundGroup.Mute;
        }

        public static void Mute(this SoundComponent soundComponent, string soundGroupName, bool mute)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return;
            }

            soundGroup.Mute = mute;
        }

        public static float GetVolume(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return 0f;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return 0f;
            }

            return soundGroup.Volume;
        }

        public static void SetVolume(this SoundComponent soundComponent, string soundGroupName, float volume)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return;
            }

            soundGroup.Volume = volume;
        }
    }
}