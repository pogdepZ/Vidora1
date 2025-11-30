using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Services;
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

    private string _savedEmail = string.Empty;
    private string _savedPassword = string.Empty;
    private bool _isUsingSavedCredentials =>
        !string.IsNullOrEmpty(_savedPassword) && _password == _savedPassword && _email == _savedEmail;

    private readonly LoginUseCase _loginUseCase;
    private readonly IUserCredentialsService _credentialsService;
    private readonly INavigationService _navigationService;
    private readonly IInfoBarService _infoBarService;
    public LoginViewModel(
        LoginUseCase loginUseCase,
        IUserCredentialsService credentialsService,
        INavigationService navigationService,
        IInfoBarService infoBarService)
    {
        _loginUseCase = loginUseCase;
        _credentialsService = credentialsService;
        _navigationService = navigationService;
        _infoBarService = infoBarService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (_isUsingSavedCredentials)
        {
            var verified = await _credentialsService.VerifyIdentityAsync();
            if (!verified)
            {
                Password = string.Empty;
                return;
            }
        }

        var request = new LoginRequest(
            Email: _email,
            Password: _password
            );

        await Task.Delay(1000);
        var result = await _loginUseCase.ExecuteAsync(request);

        if (result.IsFailure)
        {
            Password = string.Empty;
            _savedPassword = string.Empty;
            _infoBarService.ShowError(result.Error, durationMs: 3000);
            return;
        }

        if (_isRememberMe)
        {
            await _credentialsService.SaveCredentialsAsync(_email, _password);
        }
        else
        {
            await _credentialsService.ClearCredentialsAsync();
        }
    }

    [RelayCommand]
    public async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync<RegisterViewModel>(clearNavigation: true);
    }

    public async Task OnNavigatedToAsync(object? param)
    {
        if (param is string emailParam)
        {
            Email = emailParam;
            return;
        }

        var creds = await _credentialsService.GetCredentialsAsync(requireVerification: false);
        if (creds is not null)
        {
            Email = creds.Email;
            Password = creds.Password;
            IsRememberMe = true;

            _savedEmail = creds.Email;
            _savedPassword = creds.Password;
        }
    }
    public async Task OnNavigatedFromAsync()
    {

    }
}