using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using PasswordManager.Domain.Dto;
using PasswordManager.Helpers;
using PasswordManager.Hotkeys;
using PasswordManager.RestApiHelper;
using PasswordManager.Services;
using PasswordManager.Settings;
using PasswordManager.Views.MessageBox;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.ViewModels
{
    public class SettingsViewModel : ObservableRecipient
    {
        #region Design time instance
        private static readonly Lazy<SettingsViewModel> _lazy = new(GetDesignTimeVM);
        public static SettingsViewModel DesignTimeInstance => _lazy.Value;

        private static SettingsViewModel GetDesignTimeVM()
        {
            var vm = new SettingsViewModel();
            return vm;
        }
        #endregion

        private readonly ThemeService _themeService;
        private readonly AppSettingsService _appSettingsService;
        private readonly CredentialsCryptoService _credentialsCryptoService;
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly HotkeysService _hotkeysService;
        private readonly SyncService _syncService;
        private string _newPassword;
        private string _newUPassword;
        private string _newPasswordHelperText;
        private AsyncRelayCommand _changePasswordCommand;
        private AsyncRelayCommand _changeUPasswordCommand;
        private RelayCommand<System.Windows.Input.KeyEventArgs> _changeHelperPopupHotkeyCommand;
        private RelayCommand _clearShowPopupHotkeyCommand;

        private BaseTheme _themeMode;
        public BaseTheme ThemeMode
        {
            get => _themeMode;
            set
            {
                SetProperty(ref _themeMode, value);
                _themeService.ThemeMode = value;
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                SetProperty(ref _newPassword, value);
                ChangePasswordCommand.NotifyCanExecuteChanged();
            }
        }

        public string NewUPassword
        {
            get => _newUPassword;
            set
            {
                SetProperty(ref _newUPassword, value);
                ChangeUPasswordCommand.NotifyCanExecuteChanged();
            }
        }

        public string NewPasswordHelperText
        {
            get => _newPasswordHelperText;
            set => SetProperty(ref _newPasswordHelperText, value);
        }

        private Hotkey _showPopupHotkey;
        public Hotkey ShowPopupHotkey
        {
            get => _showPopupHotkey;
            set
            {
                SetProperty(ref _showPopupHotkey, value);
                ChangeHelperPopupHotkeyCommand.NotifyCanExecuteChanged();
            }
        }

        public event Action NewPasswordIsSet;

        public AsyncRelayCommand ChangePasswordCommand => _changePasswordCommand ??= new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);
        public AsyncRelayCommand ChangeUPasswordCommand => _changeUPasswordCommand ??= new AsyncRelayCommand(ChangeUPasswordAsync, CanChangeUPassword);
        public RelayCommand<System.Windows.Input.KeyEventArgs> ChangeHelperPopupHotkeyCommand => _changeHelperPopupHotkeyCommand ??= new RelayCommand<System.Windows.Input.KeyEventArgs>(ChangeHelperPopupHotkey);
        public RelayCommand ClearShowPopupHotkeyCommand => _clearShowPopupHotkeyCommand ??= new RelayCommand(ClearShowPopupHotkey);

        private SettingsViewModel() { }
        private readonly CallApi _callApi;
        public SettingsViewModel(
            ThemeService themeService,
            AppSettingsService appSettingsService,
            CredentialsCryptoService credentialsCryptoService,
            ILogger<SettingsViewModel> logger,
            HotkeysService hotkeysService,
            SyncService syncService,
            CallApi callApi)
        {
            _themeService = themeService;
            _appSettingsService = appSettingsService;
            _credentialsCryptoService = credentialsCryptoService;
            _logger = logger;
            _hotkeysService = hotkeysService;

            _themeMode = _appSettingsService.ThemeMode;
            _showPopupHotkey = _appSettingsService.ShowPopupHotkey;
            _syncService = syncService;
            _callApi = callApi;
        }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                NewPasswordHelperText = "Minimum symbols count is 8";
                return;
            }

            NewPasswordHelperText = string.Empty;
            var dialogIdentifier = MvvmHelper.MainWindowDialogName;
            var success = false;

            try
            {
                _credentialsCryptoService.SetPassword(NewPassword);
                await _credentialsCryptoService.SaveCredentials();
                await _syncService.Upload(Cloud.Enums.CloudType.TPCloud);
                NewPasswordIsSet?.Invoke();
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }

            if (success)
            {
                await MaterialMessageBox.ShowAsync(
                    "Success",
                    "New password applied",
                    MaterialMessageBoxButtons.OK,
                    dialogIdentifier,
                    PackIconKind.Tick);
            }
        }
        public string Hash(string password)
        {
            var bytes = new UTF8Encoding().GetBytes(password);
            byte[] hashBytes;
            using (var algorithm = new System.Security.Cryptography.SHA512Managed())
            {
                hashBytes = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hashBytes);
        }
        private async Task ChangeUPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUPassword) || NewUPassword.Length < 8)
            {
                NewPasswordHelperText = "Minimum symbols count is 8";
                return;
            }

            NewPasswordHelperText = string.Empty;
            var dialogIdentifier = MvvmHelper.MainWindowDialogName;
            var success = false;

            try
            {
                var loginrequest = new dtoLoginRequest { userName = "", password = Hash(App.userName+NewUPassword), FirstName = "", LastName = "" };

                string res = await _callApi.Post("api/v1.1/Authentication/changePass", loginrequest);
                //string res = await api.Post("api/v1.1/Authentication/register", loginrequest);
                var LoginRes = (dtoLoginResponse)JsonConvert.DeserializeObject(res, typeof(dtoLoginResponse));
                if (LoginRes.resCode == "000")
                {
                    NewPasswordIsSet?.Invoke();
                    success = true;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }

            if (success)
            {
                await MaterialMessageBox.ShowAsync(
                    "Success",
                    "New User password applied",
                    MaterialMessageBoxButtons.OK,
                    dialogIdentifier,
                    PackIconKind.Tick);
            }
        }
        private void ChangeHelperPopupHotkey(System.Windows.Input.KeyEventArgs args)
        {
            if (_hotkeysService.GetHotkeyForKeyPress(args, out Hotkey hotkey))
            {
                _hotkeysService.UpdateKey(hotkey, nameof(_appSettingsService.ShowPopupHotkey), HotkeyDelegates.PopupHotkeyHandler);
                ShowPopupHotkey = hotkey;
            }
        }

        private void ClearShowPopupHotkey()
        {
            ShowPopupHotkey = Hotkey.Empty;
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(NewPassword);
        }
        private bool CanChangeUPassword()
        {
            return true;
        }
    }
}
