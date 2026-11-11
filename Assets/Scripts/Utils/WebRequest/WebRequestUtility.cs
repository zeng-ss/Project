using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniFramework.WebRequest;
using UnityEngine;

public static class WebRequestUtility
{
    private static T CompletedEvent<T>(WebRequestBase webRequestBase, string response)
    {
        T bodyData = default;
        switch (webRequestBase.Status)
        {
            case EReqeustStatus.Succeed:
            {
                if (typeof(T) == typeof(string))
                {
                    bodyData = (T)(object)response;
                }
                else
                {
                    bodyData = JsonUtility.FromJson<T>(response);
                }
                break;
            }
            case EReqeustStatus.ProtocolError:
            case EReqeustStatus.ConnectionError:
            case EReqeustStatus.DataProcessingError:
            {
                Debug.LogError(
                    $"URL:{webRequestBase.URL}\nResponse:{response}\nCode:{webRequestBase.ResponseCode}\nError:{webRequestBase.RequestError}");
                break;
            }
        }

        webRequestBase.Dispose();
        return bodyData;
    }

    public static async UniTask<byte[]> DownloadData(string url, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers);

        await webRequestGet.ToUniTask();

        if (webRequestGet.Status == EReqeustStatus.Succeed)
        {
            byte[] data = webRequestGet.GetData();
            webRequestGet.Dispose();
            return data;
        }
        else
        {
            Debug.LogError($"Download failed - URL:{webRequestGet.URL}\nCode:{webRequestGet.ResponseCode}\nError:{webRequestGet.RequestError}");
            webRequestGet.Dispose();
            return null;
        }
    }

    public static async UniTask<T> Get<T>(string url, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers);

        await webRequestGet.ToUniTask();
        return CompletedEvent<T>(webRequestGet, webRequestGet.GetResponse());
    }

    public static async UniTask<T> Post<T>(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(JsonUtility.ToJson(requestBody), timeout, headers);

        await webRequestPost.ToUniTask();
        return CompletedEvent<T>(webRequestPost, webRequestPost.GetResponse());
    }

    public static async UniTask<T> Put<T>(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestPut webRequestPut = new WebRequestPut(url);
        webRequestPut.SendRequest(JsonUtility.ToJson(requestBody), timeout, headers);

        await webRequestPut.ToUniTask();
        return CompletedEvent<T>(webRequestPut, webRequestPut.GetResponse());
    }
}