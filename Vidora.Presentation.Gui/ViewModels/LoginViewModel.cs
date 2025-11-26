using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class LoginViewModel : ObservableRecipient, INavigationAware
{
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private bool _isRememberMe = false;
    public bool IsRememberMe
    {
        get => _isRememberMe;
        set => SetProperty(ref _isRememberMe, value);
    }


    private readonly LoginUseCase _loginUseCase;
    public LoginViewModel(LoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        var request = new LoginRequest(
            Email: _email,
            Password: _password
            );

        await Task.Delay(1000);
        var result = await _loginUseCase.ExecuteAsync(request);

        if (result.IsFailure)
        {
            Password = string.Empty;
            System.Diagnostics.Debug.WriteLine(result.Error);
            return;
        }

        System.Diagnostics.Debug.WriteLine("Login success");
    }

    public async Task OnNavigatedToAsync(object? param)
    {

    }
    public async Task OnNavigatedFromAsync()
    {

    }
}