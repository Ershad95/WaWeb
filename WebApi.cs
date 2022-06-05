using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using WaWeb.Models;
using WaWeb.Models.Proxy;

namespace WaWeb
{
    public class WebApi<TOutPut, TInPut> : IDisposable where TOutPut : new()
    {
        #region Field
        private readonly HttpClient _client;
        private TOutPut _result;
        private string _baseUrl;
        #endregion

        #region Ctor
        public WebApi(string baseUrl = "",
            int maxBafferSize = int.MaxValue,
            TimeSpan? timeOut = null,
            bool useProxy = false,
            ProxyInfo? proxyInfo = null)
        {
            _baseUrl = baseUrl;
            timeOut ??= TimeSpan.FromSeconds(30);
            _result = new TOutPut();
            HttpClientHandler httpClientHandler = new() ;
            httpClientHandler.UseProxy = useProxy;
            if (useProxy)
            {
                if (proxyInfo != null && !string.IsNullOrEmpty(proxyInfo.ProxyServer_port))
                {
                    // First create a proxy object
                    WebProxy proxy = new()
                    {
                        Address = new Uri(proxyInfo.ProxyServer_port),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,
                    };
                    if (!string.IsNullOrEmpty(proxyInfo.ProxyUserName) && !string.IsNullOrEmpty(proxyInfo.ProxyPassword))
                    {
                        // *** These creds are given to the proxy server, not the web server ***
                        NetworkCredential credentials = new(
                            userName: proxyInfo.ProxyUserName,
                            password: proxyInfo.ProxyPassword);
                        proxy.Credentials = credentials;
                    }
                }
                else if (proxyInfo == null)
                {
                    var proxy = GetProxyList().Result;
                    // First create a proxy object
                    WebProxy proxySelected = new()
                    {
                        Address = new Uri($"{proxy.protocols[0]}://{proxy.ip}:{proxy.port}"),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,
                    };
                    httpClientHandler.Proxy = proxySelected;

                }
            }
            _client = new(handler: httpClientHandler)
            {
                BaseAddress = new Uri(baseUrl),
                MaxResponseContentBufferSize = maxBafferSize,
                Timeout = timeOut.Value
            };
        }
        #endregion

        #region Method
        /// <summary>
        /// Just Return Fastest Proxy from Proxy List
        /// </summary>
        /// <param name="proxyRepository">do you have custom proxy List enter the url of this</param>
        /// <returns></returns>
        public static async Task<ProxyItem?> GetProxyList(
            string proxyRepository = "")
        {
            var proxyUrl = "https://proxylist.geonode.com/";
            if (!string.IsNullOrEmpty(proxyRepository))
                proxyUrl = proxyRepository;
            var api = new WebApi<ProxyResult, object>(baseUrl: proxyUrl);
            var list = await api.CallApi(new(), "api/proxy-list?limit=3&page=1&sort_by=lastChecked&sort_type=desc&speed=fast&protocols=http%2Chttps");
            return list?.data?.MinBy(x => x.speed);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        //Post Data
        public async Task<TOutPut> CallApi(
            TInPut input,
            string api,
            HttpMethods httpMethod = HttpMethods.GET)
        {
            if (string.IsNullOrEmpty(api))
                throw new Exception("Please set string of api for endpoint");

            
            //api = HttpUtility.UrlEncode(api,Encoding.UTF8);
            if (httpMethod == HttpMethods.GET)
            {
                try
                {
                    var response = await _client.GetAsync(api);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<TOutPut>(content) ?? new TOutPut();
                    }
                    else
                    {
                        throw new Exception($"Status code is {response.StatusCode}");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                var json = JsonConvert.SerializeObject(input);
                var parameter = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response;
                    switch (httpMethod)
                    {
                        case HttpMethods.POST:
                            response = await _client.PostAsync(api, parameter);
                            break;
                        case HttpMethods.PUT:
                            response = await _client.PutAsync(api, parameter);
                            break;
                        case HttpMethods.DELETE:
                            response = await _client.DeleteAsync(api);
                            break;
                        default:
                            response = new HttpResponseMessage();
                            break;
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<TOutPut>(content) ?? new TOutPut();
                    }
                    else
                    {
                        throw new Exception($"Status code is {response.StatusCode}");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return _result;

        }
        #endregion
    }
    #region NestedClass


    

   


  




    #endregion
}
