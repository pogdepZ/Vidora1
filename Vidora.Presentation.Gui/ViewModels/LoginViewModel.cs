using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Exceptions;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class LoginViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isRememberMe = false;

    private string _savedEmail = string.Empty;
    private string _savedPassword = string.Empty;
    private bool _isUsingSavedCredentials =>
        !string.IsNullOrEmpty(_savedPassword) && Password == _savedPassword && Email == _savedEmail;

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
    public async Task ExecuteLoginAsync()
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

        try
        {
            var request = new LoginCommand(
                Email: Email,
                Password: Password
                );

            var result = await _loginUseCase.ExecuteAsync(request);
        }
        catch (DomainException ex)
        {
            Password = string.Empty;
            _savedPassword = string.Empty;
            await _infoBarService.ShowErrorAsync(ex.Message, durationMs: 3000);
            return;
        }

        if (IsRememberMe)
        {
            await _credentialsService.SaveCredentialsAsync(Email, Password);
        }
        else
        {
            await _credentialsService.ClearCredentialsAsync();
        }
    }

    [RelayCommand]
    public async Task ExecuteNavigateToRegisterAsync()
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