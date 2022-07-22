using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PasswordManager.Extensions;
using PasswordManager.Helpers;
using PasswordManager.Services;
using PasswordManager.Views;
using System;
using System.Linq;
using System.Security;
using System.Windows;

namespace PasswordManager.ViewModels
{
    internal class TrayIconViewModel : ObservableRecipient
    {
        private RelayCommand _openMainWindowCommand;
        private RelayCommand _exitAppCommand;

        public RelayCommand OpenMainWindowCommand => _openMainWindowCommand ??= new RelayCommand(OpenMainWindow);
        public RelayCommand ExitAppCommand => _exitAppCommand ??= new RelayCommand(ExitApp);
        //private readonly CredentialsCryptoService _credentialsCryptoService;
        //private readonly ILogger<TrayIconViewModel> _logger;
        public TrayIconViewModel(
            //CredentialsCryptoService credentialsCryptoService,
            //ILogger<TrayIconViewModel> logger
            )
        {
            //_credentialsCryptoService = credentialsCryptoService;
            //_logger = logger;
        }
        SecureString PasswordSecure = new SecureString();
        public void SetPassword(string password)
        {
            PasswordSecure = SecureStringHelper.MakeSecureString(password);
        }

        public string GetPassword()
        {
            return SecureStringHelper.GetString(PasswordSecure);
        }


        private async void OpenMainWindow()
        {
            var dialog = new MyDialog();
            
            if (dialog.ShowDialog() == true)
            {
                CryptoService a = new CryptoService();
                CredentialsCryptoService b = new CredentialsCryptoService(a);
                
                string Password = dialog.ResponseText.Password.ToString();
                b.SetPassword(Password);
                var loadingResult = await b.LoadCredentialsAsync();
                if(loadingResult)
                {
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mainWindow?.BringToFrontAndActivate();
                }    
                
            }
            
        }

        private void ExitApp()
        {
            Application.Current.Shutdown();
        }
    }
}
