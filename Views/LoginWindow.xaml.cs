using System.Windows;
using System.Windows.Input;
using POS.ViewModels;

namespace POS.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.LoginSuccessful += OnLoginSuccessful;
        TxtUsername.Focus();
    }

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) _ = DoLogin();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
        => _ = DoLogin();

    private async Task DoLogin()
        => await _vm.ExecuteLoginWithPassword(TxtPassword.Password);

    private void OnLoginSuccessful()
    {
        var menu = new MainMenuWindow();
        menu.Show();
        Close();
    }
}
