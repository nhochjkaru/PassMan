using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PasswordManager.Authorization.Interfaces;
using PasswordManager.Authorization.Responses;
using PasswordManager.Helpers;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Authorization.Holders
{
    public class RestApiTokenHolder : ITokenHolder
    {
        private readonly ILogger _logger;

        public ITokenResponse Token { get; private set; }
        public ApiTokenResponse accesstoken { get; set; }

        public RestApiTokenHolder(ILogger<RestApiTokenHolder> logger)
        {
            _logger = logger;
            ReadToken();
        }

        private void ReadToken()
        {
            try
            {
                if (!File.Exists(Constants.ApiFilePath))
                    return;

                using var fileStream = new FileStream(Constants.ApiFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                accesstoken = System.Text.Json.JsonSerializer.Deserialize<ApiTokenResponse>(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        public async Task SetAndSaveToken(string tokenResponse, CancellationToken cancellationToken)
        {
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<ApiTokenResponse>(tokenResponse);

            if (!string.IsNullOrWhiteSpace(deserialized.AccessToken))
            {
                // Refresh token reset
                accesstoken = deserialized;
                using var fileStream = new FileStream(Constants.ApiFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                await System.Text.Json.JsonSerializer.SerializeAsync(fileStream, accesstoken as ApiTokenResponse, cancellationToken: cancellationToken);
                _logger.LogInformation("Token response saved to file");
            }
        }

        public Task RemoveToken()
        {
            try
            {
                var fileInfo = new FileInfo(Constants.ApiFilePath);
                fileInfo.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
            return Task.CompletedTask;
        }
    }
}
