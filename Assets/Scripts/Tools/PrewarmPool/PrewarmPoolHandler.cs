using CasualGame;

public class PrewarmPoolHandler : Singleton<PrewarmPoolHandler>
{
    public void InitPrewarmPool(PrewarmPoolData prewarmPoolData)
    {
        if (prewarmPoolData == null || prewarmPoolData.PrewarmPoolDataItems == null) return;

        foreach (var data in prewarmPoolData.PrewarmPoolDataItems)
        {
            if (data.Count <= 0) continue;

            LogHandler.Info($"PrewarmPool: {data.ResourceIdentificationType} {data.Count}", LogColor.Green);
            PrefabPool.Instance.PrewarmAsync((int)data.ResourceIdentificationType, data.Count);
        }
    }
}
