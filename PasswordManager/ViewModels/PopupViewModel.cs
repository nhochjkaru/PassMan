using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PasswordManager.Helpers;
using PasswordManager.Hotkeys;
using PasswordManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PasswordManager.ViewModels
{
    public class BoolToFontWeightConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((bool)value) ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    public class PopupViewModel : ObservableObject
    {
        #region Design time instance
        private static readonly Lazy<PopupViewModel> _lazy = new(GetDesignTimeVM);
        public static PopupViewModel DesignTimeInstance => _lazy.Value;

        private static PopupViewModel GetDesignTimeVM()
        {
            var vm = new PopupViewModel();
            return vm;
        }
        #endregion

        private readonly CredentialsCryptoService _credentialsCryptoService;
        private readonly ILogger<PopupViewModel> _logger;
        private readonly CredentialViewModelFactory _credentialViewModelFactory;
        private readonly UserActivityHook _userActivityHook;
        public event Action Accept;

        private ObservableCollection<CredentialViewModel> _displayedCredentials;
        public ObservableCollection<CredentialViewModel> DisplayedCredentials
        {
            get => _displayedCredentials;
            set => SetProperty(ref _displayedCredentials, value);
        }   
        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set { 
                SetProperty(ref _searchText, value);
                App.searchtext= value;
            }
        }
        private CredentialViewModel _selectedCredentialVM;
        public CredentialViewModel SelectedCredentialVM
        {
            get => _selectedCredentialVM;
            set => SetProperty(ref _selectedCredentialVM, value);
        }

        private RelayCommand<PassFieldViewModel> _setAndCloseCommand;
        public RelayCommand<PassFieldViewModel> SetAndCloseCommand => _setAndCloseCommand ??= new RelayCommand<PassFieldViewModel>(SetAndClose);

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ??= new RelayCommand(Close);
        public ObservableCollection<CredentialViewModel>  tempCred {get;set;}
        public int check { get; set; }
        private PopupViewModel() { }
        public int selectedindex { get; set; }
        public PopupViewModel(
            CredentialsCryptoService credentialsCryptoService,
            ILogger<PopupViewModel> logger,
            CredentialViewModelFactory credentialViewModelFactory,
            UserActivityHook userActivityHook)
        {
            _credentialsCryptoService = credentialsCryptoService;
            _logger = logger;
            _credentialViewModelFactory = credentialViewModelFactory;
            _userActivityHook = userActivityHook;
            
            var creds = _credentialsCryptoService.Credentials.Select(cr => _credentialViewModelFactory.ProvideNew(cr)).ToList();
            DisplayedCredentials = new ObservableCollection<CredentialViewModel>(creds);
            selectedindex = 0;
            DisplayedCredentials[selectedindex].selected=true;
            tempCred = DisplayedCredentials;
            check = 0;
            
        }
        public void addKeyEvent(bool add)
        {
            if(add)
            {
                _userActivityHook.KeyDown += _userActivityHook_KeyDown;
            }
            else
            {
                _userActivityHook.KeyDown -= _userActivityHook_KeyDown;
            }
        }
        private void Close()
        {
            //_userActivityHook.KeyDown -= _userActivityHook_KeyDown;
            Accept?.Invoke();
        }

        private void _userActivityHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            List<CredentialViewModel> creds = null;
            bool isLetterOrDigit = char.IsLetterOrDigit((char)e.KeyCode);
            if(isLetterOrDigit)
            {
                SearchText += e.KeyData.ToString().ToLower();
                creds = tempCred.Where(n => (n.LoginFieldVM.Value==null?false: n.LoginFieldVM.Value.Contains(SearchText)) || (n.NameFieldVM.Value == null ? false : n.NameFieldVM.Value.Contains(SearchText)) || (n.OtherFieldVM.Value == null ? false : n.OtherFieldVM.Value.Contains(SearchText)) || (n.SiteFieldVM.Value == null ? false : n.SiteFieldVM.Value.Contains(SearchText))).ToList();
                DisplayedCredentials = new ObservableCollection<CredentialViewModel>(creds);
                setSelectedIndex(creds);
            }
            //else if (e.KeyCode == System.Windows.Forms.Keys.Space)
            //{
            //    SearchText += " ";
            //    creds = tempCred.Where(n => (n.LoginFieldVM.Value == null ? false : n.LoginFieldVM.Value.Contains(SearchText)) || (n.NameFieldVM.Value == null ? false : n.NameFieldVM.Value.Contains(SearchText)) || (n.OtherFieldVM.Value == null ? false : n.OtherFieldVM.Value.Contains(SearchText)) || (n.SiteFieldVM.Value == null ? false : n.SiteFieldVM.Value.Contains(SearchText))).ToList();
            //    DisplayedCredentials = new ObservableCollection<CredentialViewModel>(creds);
            //}
            //else if(e.KeyCode == System.Windows.Forms.Keys.Back)
            //{
            //    SearchText = "";
            //    DisplayedCredentials = new ObservableCollection<CredentialViewModel>(tempCred);
            //}
            else
            {
                switch(e.KeyCode)
                {
                    case System.Windows.Forms.Keys.Space:
                        SearchText += " ";
                        creds = tempCred.Where(n => (n.LoginFieldVM.Value == null ? false : n.LoginFieldVM.Value.Contains(SearchText)) || (n.NameFieldVM.Value == null ? false : n.NameFieldVM.Value.Contains(SearchText)) || (n.OtherFieldVM.Value == null ? false : n.OtherFieldVM.Value.Contains(SearchText)) || (n.SiteFieldVM.Value == null ? false : n.SiteFieldVM.Value.Contains(SearchText))).ToList();
                        DisplayedCredentials = new ObservableCollection<CredentialViewModel>(creds);
                        setSelectedIndex(creds);
                        break;
                    case System.Windows.Forms.Keys.Back:
                        SearchText = "";
                        DisplayedCredentials = new ObservableCollection<CredentialViewModel>(tempCred);
                        setSelectedIndex(creds);
                        break;

                    case System.Windows.Forms.Keys.Escape:
                       // _userActivityHook.KeyDown -= _userActivityHook_KeyDown;
                        Accept?.Invoke();
                        break;
                    case System.Windows.Forms.Keys.Down:
                        selectedindex++;
                        setSelectedIndex(creds);
                        break;
                    case System.Windows.Forms.Keys.Up:
                        selectedindex--;
                        setSelectedIndex(creds);
                        break;
                    case System.Windows.Forms.Keys.Right:
                        SetAndClose(DisplayedCredentials[selectedindex].PasswordFieldVM);
                        break;
                    case System.Windows.Forms.Keys.Left:
                        SetAndClose(DisplayedCredentials[selectedindex].LoginFieldVM);
                        break;
                    default:
                        break;
                }
            }

            
        }
        private void setSelectedIndex(List<CredentialViewModel> creds)
        {
            _logger.LogInformation($"setSelectedIndex last Selected index = {selectedindex}");
            var templist = creds!=null? creds.Select(c => { c.selected = false; return c; }).ToList() : DisplayedCredentials.Select(c => { c.selected = false; return c; }).ToList();
            if (templist.Count > 0)
            {
                _logger.LogInformation($"setSelectedIndex new Selected templist.Count = {templist.Count}");
                if (selectedindex > templist.Count-1)
                {
                    selectedindex = templist.Count-1;
                }
                if (selectedindex < 0)
                {
                    selectedindex = 0;
                }
                _logger.LogInformation($"setSelectedIndex new Selected index = {selectedindex}");
                templist[selectedindex].selected = true;
                DisplayedCredentials = new ObservableCollection<CredentialViewModel>(templist);
            }
        }
        private void SetAndClose(PassFieldViewModel passFieldViewModel)
        {
            try
            {
                var inputData = passFieldViewModel.Value;
                if (!string.IsNullOrWhiteSpace(inputData))
                {
                    string tempclipboard= WindowsClipboard.GetText();
                    WindowsClipboard.SetText(inputData);

                    INPUT[] inputsa = new INPUT[4];

                    inputsa[0].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputsa[0].U.ki.wVk = WindowsKeyboard.VK_CONTROL;

                    inputsa[1].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputsa[1].U.ki.wVk = WindowsKeyboard.VK_A;

                    inputsa[2].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputsa[2].U.ki.wVk = WindowsKeyboard.VK_A;
                    inputsa[2].U.ki.dwFlags = WindowsKeyboard.KEYEVENTF_KEYUP;

                    inputsa[3].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputsa[3].U.ki.wVk = WindowsKeyboard.VK_CONTROL;
                    inputsa[3].U.ki.dwFlags = WindowsKeyboard.KEYEVENTF_KEYUP;

                    // Send input simulate Ctrl + V
                    var uSenta = WindowsKeyboard.SendInput((uint)inputsa.Length, inputsa, INPUT.Size);


                    INPUT[] inputs = new INPUT[4];

                    inputs[0].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputs[0].U.ki.wVk = WindowsKeyboard.VK_CONTROL;

                    inputs[1].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputs[1].U.ki.wVk = WindowsKeyboard.VK_V;

                    inputs[2].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputs[2].U.ki.wVk = WindowsKeyboard.VK_V;
                    inputs[2].U.ki.dwFlags = WindowsKeyboard.KEYEVENTF_KEYUP;

                    inputs[3].type = WindowsKeyboard.INPUT_KEYBOARD;
                    inputs[3].U.ki.wVk = WindowsKeyboard.VK_CONTROL;
                    inputs[3].U.ki.dwFlags = WindowsKeyboard.KEYEVENTF_KEYUP;

                    // Send input simulate Ctrl + V
                    var uSent = WindowsKeyboard.SendInput((uint)inputs.Length, inputs, INPUT.Size);


                    var t = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        WindowsClipboard.SetText(tempclipboard);
                    });
                    //t.RunSynchronously();
                }
                //_userActivityHook.KeyDown -= _userActivityHook_KeyDown;
                Accept?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
        }
    }
}
