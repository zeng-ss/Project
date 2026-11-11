#if UNITY_EDITOR
public class ResourcesIdentificationPostprocessor : UnityEditor.AssetPostprocessor
{
    private void OnPreprocessAsset()
    {
        if(assetPath.Contains("#"))
        {
            ResourceIdentificationTool.InitResourceIdentificationInfoMapInProject();
        }
    }
} 
#endif

