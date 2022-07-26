using Newtonsoft.Json;
using PasswordManager.Authorization.Brokers;
using PasswordManager.Authorization.Interfaces;
using PasswordManager.Clouds.Interfaces;
using PasswordManager.Clouds.Models;
using PasswordManager.Domain.Dto;
using PasswordManager.Helpers;
using PasswordManager.RestApiHelper;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Clouds.Services
{
    public class RestApiCloudService : ICloudService
    {
        public IAuthorizationBroker AuthorizationBroker => null;
        private readonly CallApi _api;

        public RestApiCloudService(CallApi api)
        {
            _api=api;
        }

        

        public async Task Upload(Stream stream,string filename, CancellationToken cancellationToken)
        {
            var bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            await stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Dispose();
            var base64urlContent = Jose.Base64Url.Encode(bytes);
            //CallApi api = new CallApi();
            var uploadrequest = new dtoUploadRequest { data= base64urlContent };
            string res = await _api.Post("api/v1.1/Sync/Upload", uploadrequest).ConfigureAwait(false);
            var LoginRes = (dtoUploadResponse)JsonConvert.DeserializeObject(res, typeof(dtoUploadResponse));
            if (_api.IsSuccessStatusCode)
            {
                if (LoginRes.resCode == "000")
                {

                }
            }

        }

        public async Task<Stream> Download(string fileName, CancellationToken cancellationToken)
        {
            //todo call api download file
            //CallApi api = new CallApi();
            var uploadrequest = new dtoSyncRequest {request="1" };
            string res = await _api.Post("api/v1.1/Sync/Sync", uploadrequest).ConfigureAwait(false);
            var LoginRes = (dtoSyncResponse)JsonConvert.DeserializeObject(res, typeof(dtoSyncResponse));
            if (_api.IsSuccessStatusCode)
            {
                if (LoginRes.resCode == "000")
                {
                    var fileContent = Jose.Base64Url.Decode(LoginRes.data);

                    return new MemoryStream(fileContent);
                }
            }
            throw new System.Exception();
        }

        public async Task<BaseUserInfo> GetUserInfo(CancellationToken cancellationToken)
        {
            //TODO call api get info
            //var jsonResponse = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            //var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(jsonResponse);
            //return userInfo.ToBaseUserInfo();

            return null;
        }
    }
}
