using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniFramework.WebRequest
{
	public sealed class WebRequestPut : WebRequestBase
	{
		public WebRequestPut(string url) : base(url)
		{
		}

		/// <summary>
		/// 发送PUT请求
		/// </summary>
		/// <param name="put">PUT的文本内容</param>
		/// <param name="timeout">超时：从请求开始计时</param>
		public void SendRequest(string put, int timeout = 0, Dictionary<string, string> headers = null)
		{
			// Check error
			// if (string.IsNullOrEmpty(put))
			// 	throw new Exception($"Web put content is null or empty : {URL}");

			if (_webRequest == null)
			{
				_webRequest = UnityWebRequest.Put(URL, put);
				SetRequestHeader(headers);
				SendRequestInternal(timeout);
			}
		}

		/// <summary>
		/// 获取响应的文本数据
		/// </summary>
		public string GetResponse()
		{
			if (_webRequest != null && IsDone())
				return _webRequest.downloadHandler.text;
			else
				return null;
		}

		private void SendRequestInternal(int timeout)
		{
			DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
			_webRequest.downloadHandler = handler;
			_webRequest.disposeDownloadHandlerOnDispose = true;
			_webRequest.timeout = timeout;
			_operation = _webRequest.SendWebRequest();
			_operation.completed += CompleteInternal;
		}
	}
}