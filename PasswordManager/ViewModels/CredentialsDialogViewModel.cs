using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PasswordManager.Enums;
using PasswordManager.Helpers;
using PasswordManager.Models;
using PasswordManager.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PasswordManager.ViewModels
{
    public class CredentialsDialogViewModel : ObservableRecipient
    {
        #region Design time instance
        private static readonly Lazy<CredentialsDialogViewModel> _lazy = new(GetDesignTimeVM);
        public static CredentialsDialogViewModel DesignTimeInstance => _lazy.Value;
        private static CredentialsDialogViewModel GetDesignTimeVM()
        {
            var additionalFields = new List<PassField>() { new PassField() { Name = "Design additional field", Value = "Test value" } };
            var model = Credential.CreateNew();
            model.AdditionalFields = additionalFields;

            var credVm = new CredentialViewModel(model, null);
            var vm = new CredentialsDialogViewModel(null,null);
            vm._credentialViewModel = credVm;
            vm.Mode = CredentialsDialogMode.View;
            return vm;
        }
        #endregion

        private readonly ILogger<CredentialsDialogViewModel> _logger;
        private readonly PasswordGenerater _passwordGenerater;
        public event Action<CredentialViewModel, CredentialsDialogMode> Accept;
        public event Action<CredentialViewModel> Delete;
        public event Action Cancel;

        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _editCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _openInBrowserCommand;
        private RelayCommand<string> _copyToClipboardCommand;
        private RelayCommand _generatePasswordCommand;
        private CredentialViewModel _credentialViewModel;
        private CredentialsDialogMode _mode = CredentialsDialogMode.View;
        private bool _isPasswordVisible;
        private bool _isNameTextBoxFocused;

        public RelayCommand OkCommand => _okCommand ??= new RelayCommand(OkExecute);
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(CancelExecute);
        public RelayCommand EditCommand => _editCommand ??= new RelayCommand(EditExecute);
        public RelayCommand DeleteCommand => _deleteCommand ??= new RelayCommand(DeleteExecute);
        public RelayCommand<string> CopyToClipboardCommand => _copyToClipboardCommand ??= new RelayCommand<string>(CopyToClipboard);
        public RelayCommand GeneratePasswordCommand => _generatePasswordCommand ??= new RelayCommand(GeneratePassword);
        public RelayCommand OpenInBrowserCommand => _openInBrowserCommand ??= new RelayCommand(OpenInBrowser);

        public CredentialViewModel CredentialViewModel
        {
            get => _credentialViewModel;
            set => SetProperty(ref _credentialViewModel, value);
        }

        public string CaptionText
        {
            get
            {
                switch (_mode)
                {
                    case CredentialsDialogMode.Edit:
                        return PasswordManager.Language.Properties.Resources.Edit;
                    case CredentialsDialogMode.New:
                        return PasswordManager.Language.Properties.Resources.New;
                    case CredentialsDialogMode.View:
                        return PasswordManager.Language.Properties.Resources.Details;
                    default:
                        break;
                }

                return string.Empty;
            }
        }

        public CredentialsDialogMode Mode
        {
            get => _mode;
            set
            {
                SetProperty(ref _mode, value);
                OnPropertyChanged(nameof(CaptionText));
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public bool IsNameTextBoxFocused
        {
            get => _isNameTextBoxFocused;
            set => SetProperty(ref _isNameTextBoxFocused, value);
        }

        public CredentialsDialogViewModel(ILogger<CredentialsDialogViewModel> logger,
            PasswordGenerater passwordGenerater)
        {
            _logger = logger;
            _passwordGenerater = passwordGenerater;
        }

        private void OkExecute()
        {
            CredentialViewModel.NameFieldVM.ValidateValue();
            if (CredentialViewModel.NameFieldVM.HasErrors)
                return;

            Accept?.Invoke(CredentialViewModel, Mode);
        }

        private void CancelExecute()
        {
            Cancel?.Invoke();
        }

        private void EditExecute()
        {
            if (CredentialViewModel is null)
                return;

            CredentialViewModel = CredentialViewModel.Clone();
            Mode = CredentialsDialogMode.Edit;
            IsPasswordVisible = true;
            SetFocus();
        }

        private void DeleteExecute()
        {
            if (CredentialViewModel is null)
                return;

            Delete?.Invoke(CredentialViewModel);
        }

        private void CopyToClipboard(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            try
            {
                WindowsClipboard.SetText(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }
        private void GeneratePassword()
        {
            try
            {
                CredentialViewModel.PasswordFieldVM.Value = _passwordGenerater.GenerateRandomStrongPassword(16);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }
       
        private void OpenInBrowser()
        {
            var uri = CredentialViewModel?.SiteFieldVM?.Value;
            if (string.IsNullOrWhiteSpace(uri))
                return;

            uri = uri.Replace("&", "^&");
            if (Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var site))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {site}") { CreateNoWindow = true });
            }
        }

        public void SetFocus()
        {
            IsNameTextBoxFocused = false;
            IsNameTextBoxFocused = true;
        }
    }
}
