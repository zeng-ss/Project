using UnityEngine;
using UnityEditor;

public class AudioPostprocessor : AssetPostprocessor
{
    private AudioImporterSampleSettings settingsOther;
    private AudioImporterSampleSettings settingsEffect;
    private AudioImporterSampleSettings settingsMusic;
    private AudioImporterSampleSettings OggLoad;

    public AudioPostprocessor()
    {
        settingsOther = new AudioImporterSampleSettings();
        settingsOther.loadType = AudioClipLoadType.CompressedInMemory;
        settingsOther.compressionFormat = AudioCompressionFormat.Vorbis;
        settingsOther.quality = 0.2f;
        settingsOther.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
        settingsOther.sampleRateOverride = 16000; // 16k

        settingsEffect = new AudioImporterSampleSettings();
        settingsEffect.loadType = AudioClipLoadType.DecompressOnLoad;
        settingsEffect.compressionFormat = AudioCompressionFormat.PCM;
        settingsEffect.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
        settingsEffect.sampleRateOverride = 16000; // 16k

        settingsMusic = new AudioImporterSampleSettings();
        settingsMusic.loadType = AudioClipLoadType.Streaming;
        settingsMusic.compressionFormat = AudioCompressionFormat.Vorbis;
        settingsMusic.quality = 0.2f;
        settingsMusic.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
        settingsMusic.sampleRateOverride = 16000; // 16k

        OggLoad = new AudioImporterSampleSettings();
        OggLoad.compressionFormat = AudioCompressionFormat.Vorbis;
        OggLoad.loadType = AudioClipLoadType.CompressedInMemory;
        OggLoad.quality = 0.02f;
        OggLoad.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
    }

    public void OnPreprocessAudio()
    {
        string name = assetPath.Substring(assetPath.LastIndexOf("/") + 1);

        AudioImporter importer = (AudioImporter)assetImporter;
        importer.forceToMono = true;

        if (name.Contains("_OGGLOAD"))
        {
            importer.defaultSampleSettings = OggLoad;
        }
        else if (name.Contains("_SFX"))
        {
            importer.defaultSampleSettings = settingsEffect;
        }
        else if (name.Contains("_MUSIC"))
        {
            importer.defaultSampleSettings = settingsMusic;
        }
        else if (name.Contains("_CUSTOM"))
        {

        }
        else
        {
            importer.defaultSampleSettings = settingsOther;
        }
    }

}
