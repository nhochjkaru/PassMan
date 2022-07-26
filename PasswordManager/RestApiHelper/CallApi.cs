using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PasswordManager.Authorization.Holders;
using PasswordManager.Authorization.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.RestApiHelper
{
    public class ApiConfig
    {
        public string baseUrl { get; init; }
    }
    public class CallApi
    {
        public HttpClient identityAPI { get; set; } 
        public HttpStatusCode StatusCode { get; set; } 
        public bool IsSuccessStatusCode { get; set; }
        private readonly ApiConfig _config;
        private readonly RestApiTokenHolder _tokenHolder;
        public CallApi(IOptions<ApiConfig> options, RestApiTokenHolder tokenHolder)
        {
            _tokenHolder = tokenHolder;
            _config = options.Value;

            ApiTokenResponse token = _tokenHolder.accesstoken;

            identityAPI = new HttpClient();
            identityAPI.BaseAddress = new Uri(_config.baseUrl);
            identityAPI.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        public async Task<string> Post<T>(string Url, T obj)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            StringContent data = new StringContent(ClassToJsonString<T> (obj), Encoding.UTF8, "application/json");
            result = await identityAPI.PostAsync(Url, data);
            StatusCode=result.StatusCode;
            IsSuccessStatusCode = result.IsSuccessStatusCode;
            if(StatusCode == HttpStatusCode.OK)
            {
                return result.Content.ReadAsStringAsync().Result;
            }
            return "";
        }

        public async Task<string> Get(string Url)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            result = await identityAPI.GetAsync(Url);
            StatusCode = result.StatusCode;
            IsSuccessStatusCode = result.IsSuccessStatusCode;
            if (StatusCode == HttpStatusCode.OK)
            {
                return result.Content.ReadAsStringAsync().Result;
            }
            return "";
        }
        public string ClassToJsonString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
