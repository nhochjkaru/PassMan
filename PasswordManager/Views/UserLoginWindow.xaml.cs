using PasswordManager.Controls;
using PasswordManager.ViewModels;
using System;
using System.Windows.Controls;

namespace PasswordManager.Views
{
    /// <summary>
    /// Interaction logic for UserLoginWindow.xaml
    /// </summary>
    public partial class UserLoginWindow : MaterialWindow
    {
        private UserLoginWindowViewModel ViewModel => DataContext as UserLoginWindowViewModel;
        public UserLoginWindow(UserLoginWindowViewModel userLoginWindowViewModel)
        {
            InitializeComponent();
            DataContext = userLoginWindowViewModel;
            userLoginWindowViewModel.Accept += UserLoginWindowViewModel_Accept;
      
        }

        private void UserLoginWindowViewModel_Accept()
        {
            DialogResult = true;
        }
        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordBox)
                return;

            ViewModel.Password = passwordBox.Password;
        }
        private void UserBox_UserChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is not TextBox userBox)
                return;

            ViewModel.UserName = userBox.Text;
        }
        private void MaterialWindow_Closed(object sender, System.EventArgs e)
        {
            ViewModel.Accept -= UserLoginWindowViewModel_Accept;
        }

        private void MaterialWindow_Activated(object sender, System.EventArgs e)
        {
            ViewModel.RefreshCapsLockCommand.Execute(null);
        }
    }
}
