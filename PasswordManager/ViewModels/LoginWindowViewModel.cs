using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PasswordManager.Helpers;
using PasswordManager.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.ViewModels
{
    public class LoginWindowViewModel : ObservableRecipient
    {
        #region Design time instance
        private static readonly Lazy<LoginWindowViewModel> _lazy = new Lazy<LoginWindowViewModel>(() => new LoginWindowViewModel(null,null, null));
        public static LoginWindowViewModel DesignTimeInstance => _lazy.Value;
        #endregion

        public event Action Accept;

        private readonly CredentialsCryptoService _credentialsCryptoService;
        private readonly ILogger<LoginWindowViewModel> _logger;
        private readonly SyncService _syncService;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _credentialsFileExist;

        private AsyncRelayCommand _loadCredentialsCommand;
        public AsyncRelayCommand LoadCredentialsCommand => _loadCredentialsCommand ??= new AsyncRelayCommand(LoadCredentialsAsync);

        private AsyncRelayCommand _loadingCommand;
        public AsyncRelayCommand LoadingCommand => _loadingCommand ??= new AsyncRelayCommand(LoadingAsync);

        private RelayCommand<KeyEventArgs> _refreshCapsLockCommand;
        public RelayCommand<KeyEventArgs> RefreshCapsLockCommand => _refreshCapsLockCommand ??= new RelayCommand<KeyEventArgs>(RefreshCapsLock);

        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        private string _helperText;
        public string HelperText
        {
            get => _helperText;
            set => SetProperty(ref _helperText, value);
        }

        private string _hintText = PasswordManager.Language.Properties.Resources.Password;
        public string HintText
        {
            get => _hintText;
            set => SetProperty(ref _hintText, value);
        }

        public bool IsCapsLockEnabled => Console.CapsLock;

        public string Password { get; set; }

        public LoginWindowViewModel(
            CredentialsCryptoService credentialsCryptoService,
            ILogger<LoginWindowViewModel> logger,
            SyncService syncService)
        {
            _credentialsCryptoService = credentialsCryptoService;
            _logger = logger;
            _syncService = syncService;
        }
        private async Task<string> SyncPasswordRequired()
        {
            return Password;
        }
        public async Task LoadCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                HelperText = PasswordManager.Language.Properties.Resources.Minimum8Characters;
                return;
            }
            else
            {
                HelperText = string.Empty;
            }

            try
            {
                Loading = true;
                if (!_credentialsFileExist)
                {
                    _credentialsCryptoService.SetPassword(Password);
                    await _syncService.Synchronize(Cloud.Enums.CloudType.TPCloud, SyncPasswordRequired);
                    Accept?.Invoke();
                    
                }
                else
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new();
                    var cancellationToken = _cancellationTokenSource.Token;

                    _credentialsCryptoService.SetPassword(Password);
                    var loadingResult = await _credentialsCryptoService.LoadCredentialsAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!loadingResult)
                    {
                        HelperText = PasswordManager.Language.Properties.Resources.PasswordIsIncorrect;
                    }
                    else
                    {
                        await _syncService.Synchronize(Cloud.Enums.CloudType.TPCloud, SyncPasswordRequired);
                        Accept?.Invoke();
                        
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task LoadingAsync()
        {
            _credentialsFileExist = await _credentialsCryptoService.IsCredentialsFileExistAsync();
            if (!_credentialsFileExist)
            {
                HintText = PasswordManager.Language.Properties.Resources.NewPassword;
            }
        }

        private void RefreshCapsLock(KeyEventArgs args)
        {
            if (args is null || args.Key == Key.CapsLock)
                OnPropertyChanged(nameof(IsCapsLockEnabled));
        }
    }
}
