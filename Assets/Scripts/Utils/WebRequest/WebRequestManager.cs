using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

internal partial class WebRequestManager
{
    private Dictionary<string, string> _headers = new Dictionary<string, string>()
    {
        {"Content-Type", "application/json"}
    };

    public void AddHeader(string key, string value)
    {
        _headers[key] = value;
    }

    public async UniTask<T> Get<T>(string url, int timeout = 30, int retryCount = 3)
    {
        string fullURL = BaseURL.URL + url;

        var webRequestBase = await WebRequestUtility.Get<BaseModel<T>>(fullURL, timeout, _headers);

        if (webRequestBase == null || webRequestBase.data == null)
        {
            return default(T);
        }

        return webRequestBase.data;
    }

    public async UniTask<T> Post<T>(string url, object requestBody, int timeout = 30, int retryCount = 3)
    {
        string fullURL = BaseURL.URL + url;

        var webRequestBase = await WebRequestUtility.Post<BaseModel<T>>(fullURL, requestBody, timeout, _headers);

        if (webRequestBase == null || webRequestBase.data == null)
        {
            return default(T);
        }

        return webRequestBase.data;
    }

    public async UniTask<T> Put<T>(string url, object requestBody, int timeout = 30, int retryCount = 3)
    {
        string fullURL = BaseURL.URL + url;

        var webRequestBase = await WebRequestUtility.Put<BaseModel<T>>(fullURL, requestBody, timeout, _headers);

        if (webRequestBase == null || webRequestBase.data == null)
        {
            return default(T);
        }

        return webRequestBase.data;
    }
}

[Serializable]
public class BaseModel<T>
{
    public T data;

    public int code;

    public string message;

    public long time_unix;

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}
