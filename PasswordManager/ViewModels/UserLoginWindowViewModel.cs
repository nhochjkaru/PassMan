using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using PasswordManager.Authorization.Holders;
using PasswordManager.Authorization.Responses;
using PasswordManager.Domain.Dto;
using PasswordManager.Helpers;
using PasswordManager.RestApiHelper;
using PasswordManager.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.ViewModels
{
    public class UserLoginWindowViewModel : ObservableRecipient
    {
        #region Design time instance
        private static readonly Lazy<UserLoginWindowViewModel> _lazy = new Lazy<UserLoginWindowViewModel>(() => new UserLoginWindowViewModel(null, null, null, null));
        public static UserLoginWindowViewModel DesignTimeInstance => _lazy.Value;
        #endregion

        public event Action Accept;

        private readonly CredentialsCryptoService _credentialsCryptoService;
        private readonly ILogger<LoginWindowViewModel> _logger;
        private readonly RestApiTokenHolder _tokenHolder;
        private readonly CallApi _callApi;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _credentialsFileExist;
        //PushCredentialsCommand
        private AsyncRelayCommand _pushCredentialsCommand;
        public AsyncRelayCommand PushCredentialsCommand => _pushCredentialsCommand ??= new AsyncRelayCommand(PushCredentialsAsync);


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

        private string _userNameText = PasswordManager.Language.Properties.Resources.UserName;
        public string UserNameText
        {
            get => _userNameText;
            set => SetProperty(ref _userNameText, value);
        }
        public bool IsCapsLockEnabled => Console.CapsLock;
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserLoginWindowViewModel(
            CredentialsCryptoService credentialsCryptoService,
            ILogger<LoginWindowViewModel> logger,
            RestApiTokenHolder tokenHolder,
            CallApi callApi)
        {
            _credentialsCryptoService = credentialsCryptoService;
            _logger = logger;
            _tokenHolder = tokenHolder;
            _callApi = callApi;


        }

        public async Task PushCredentialsAsync()
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
                //TODO call register API
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new();
                var cancellationToken = _cancellationTokenSource.Token;

                //_credentialsCryptoService.SetPassword(Password);
                var loadingResult = await _credentialsCryptoService.LoadCredentialsAsync();
                cancellationToken.ThrowIfCancellationRequested();
                var loginrequest = new dtoLoginRequest { userName = UserName, password = Password, FirstName = "", LastName = "" };

                string res = await _callApi.Post("api/v1.1/Authentication/register", loginrequest);
                //string res = await api.Post("api/v1.1/Authentication/register", loginrequest);
                var LoginRes = (dtoLoginResponse)JsonConvert.DeserializeObject(res, typeof(dtoLoginResponse));
                if (LoginRes.resCode == "000")
                {
                    App.apitoken = LoginRes.token;
                    App.credSt = null;
                    ApiTokenResponse a = new ApiTokenResponse();
                    a.AccessToken = LoginRes.token;
                    a.Username = UserName;
                    await _tokenHolder.SetAndSaveToken(JsonConvert.SerializeObject(a, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), cancellationToken);
                    Accept?.Invoke();

                    //TODO load LoadCredentials
                }
                else
                {
                    HelperText = PasswordManager.Language.Properties.Resources.PasswordIsIncorrect;

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

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new();
                var cancellationToken = _cancellationTokenSource.Token;

                //_credentialsCryptoService.SetPassword(Password);
                var loadingResult = await _credentialsCryptoService.LoadCredentialsAsync();
                cancellationToken.ThrowIfCancellationRequested();

                //TODO call login API
                var loginrequest = new dtoLoginRequest { userName = UserName, password = Password, FirstName = "", LastName = "" };
                string res = await _callApi.Post("api/v1.1/Authentication/login", loginrequest);
                //string res = await api.Post("api/v1.1/Authentication/login", loginrequest);
                var LoginRes = (dtoLoginResponse)JsonConvert.DeserializeObject(res, typeof(dtoLoginResponse));
                if (LoginRes.resCode == "000")
                {
                    App.apitoken = LoginRes.token;
                    App.credSt = null;

                    ApiTokenResponse a = new ApiTokenResponse();
                    a.AccessToken = LoginRes.token;
                    a.Username = UserName;
                    await _tokenHolder.SetAndSaveToken(JsonConvert.SerializeObject(a, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), cancellationToken);
                    Accept?.Invoke();

                    //TODO load LoadCredentials
                }
                else
                {
                    HelperText = PasswordManager.Language.Properties.Resources.PasswordIsIncorrect;

                }
                //if (!_credentialsFileExist)
                //{
                //    _credentialsCryptoService.SetPassword(Password);
                //    Accept?.Invoke();
                //}
                //else
                //{
                //    _cancellationTokenSource?.Cancel();
                //    _cancellationTokenSource = new();
                //    var cancellationToken = _cancellationTokenSource.Token;

                //    _credentialsCryptoService.SetPassword(Password);
                //    var loadingResult = await _credentialsCryptoService.LoadCredentialsAsync();
                //    cancellationToken.ThrowIfCancellationRequested();
                //    if (!loadingResult)
                //    {
                //        HelperText = PasswordManager.Language.Properties.Resources.PasswordIsIncorrect;
                //    }
                //    else
                //    {
                //        Accept?.Invoke();
                //    }
                //}
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
                UserNameText = PasswordManager.Language.Properties.Resources.UserName;
            }
        }

        private void RefreshCapsLock(KeyEventArgs args)
        {
            if (args is null || args.Key == Key.CapsLock)
                OnPropertyChanged(nameof(IsCapsLockEnabled));
        }
    }
}
