using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using PasswordManager.Helpers.Threading;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Media;

namespace PasswordManager.Services
{
    public class FavIconService
    {
        private const string _favIconServiceUrl = "http://www.google.com/s2/favicons?domain_url={0}";
        private readonly ConcurrentDictionary<string, ImageSource> _imagesDict = new();
        private readonly ILogger<FavIconService> _logger;
        private readonly ImageService _imageService;
        private readonly AsyncKeyedLocker<string> _asyncKeyedLock;

        public FavIconService(
            ILogger<FavIconService> logger,
            ImageService imageService,
            AsyncKeyedLocker<string> asyncKeyedLock)
        {
            _imageService = imageService;
            _logger = logger;
            _asyncKeyedLock = asyncKeyedLock;
        }

        public ImageSource GetImage(string imageUrlString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrlString) || !Uri.TryCreate(imageUrlString, UriKind.RelativeOrAbsolute, out Uri imageUrl))
                    return null;

                var host = imageUrl.Host;
                ImageSource bitmapImage;

                using var locker = _asyncKeyedLock.Lock(host);
                if (_imagesDict.TryGetValue(host, out ImageSource image))
                {
                    bitmapImage = image;
                }
                else
                {
                    bitmapImage = _imageService.GetImageAsync(string.Format(_favIconServiceUrl, host), CancellationToken.None).Result;
                    _imagesDict.TryAdd(host, bitmapImage);
                }

                return bitmapImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return null;
            }
        }
    }
}
