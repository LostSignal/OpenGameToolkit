//-----------------------------------------------------------------------
// <copyright file="HttpUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class HttpUtil
    {
        public static Task<HttpWebResponse> SendJsonPost(string uri, string jsonData, string basicAuth = null)
        {
            return SendJsonPost(new Uri(uri), jsonData, basicAuth);
        }

        public static async Task<HttpWebResponse> SendJsonPost(Uri uri, string jsonData, string basicAuth = null)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);

            // Constructing the WebResponse
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = data.Length;
            webRequest.KeepAlive = true;
            webRequest.Headers.Clear();

            // Adding basic auth header (if exists)
            if (string.IsNullOrEmpty(basicAuth) == false)
            {
                webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {basicAuth}");
            }

            using (var requestStream = await webRequest.GetRequestStreamAsync())
            {
                requestStream.Write(data, 0, data.Length);
            }

            return (await webRequest.GetResponseAsync()) as HttpWebResponse;
        }
    }
}
