using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityGameFramework.Runtime;
using CasualGame;
using HorstRill.Data;
using Newtonsoft.Json;

[OPS.Obfuscator.Attribute.DoNotObfuscateClass]
public class RemoteConfigManager : Singleton<RemoteConfigManager>
{
    //remoteConfig相关
    private const string FirebaseRemoteKey = "FirebaseRemoteKey_";
    private bool isFirebaseFetchCompleted = false;
    private bool isFirebaseFetchSucceeded = false;
    private bool isFirebaseUpdated = false;
    public bool IsFirebaseFetchCompleted => isFirebaseFetchCompleted;

    [Serializable]
    private class FirebaseFetchResult
    {
        public bool success;
        public bool updated;
    }

    public string GetStringValue(string key, string defaultValue)
    {
        if (!isFirebaseFetchSucceeded)
        {
            return GetCachedOrDefaultValue(key, defaultValue);
        }

        string value = HorstRillDataManager.Instance.GetFirebaseRemoteConfigStringValue(key);
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("==> Firebase RemoteConfig value is empty, use cached/default value: " + key);
            return GetCachedOrDefaultValue(key, defaultValue);
        }

        PlayerPrefs.SetString(FirebaseRemoteKey + key, value);
        Debug.Log("==> Firebase RemoteConfig get value: " + key + " = " + value);

        return value;
    }

    private string GetCachedOrDefaultValue(string key, string defaultValue)
    {
        if (PlayerPrefs.HasKey(FirebaseRemoteKey + key))
        {
            return PlayerPrefs.GetString(FirebaseRemoteKey + key);
        }

        return defaultValue;
    }

    public void OnFetchCompleted(string resultValue)
    {
        isFirebaseFetchCompleted = true;
        isFirebaseFetchSucceeded = false;
        isFirebaseUpdated = false;

        try
        {
            if (!string.IsNullOrEmpty(resultValue))
            {
                FirebaseFetchResult result = JsonConvert.DeserializeObject<FirebaseFetchResult>(resultValue);
                if (result != null)
                {
                    isFirebaseFetchSucceeded = result.success;
                    isFirebaseUpdated = result.updated;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("==> Firebase RemoteConfig fetch result parse failed: " + e.Message);
        }

        Debug.Log(
            $"==> Firebase RemoteConfig fetch completed, " +
            $"success {isFirebaseFetchSucceeded}, updated {isFirebaseUpdated}");
    }
}
