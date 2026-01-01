using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Vidora.Core.Dtos.Requests;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class RegisterViewModel : ObservableRecipient, INavigationAware
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

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }


    private readonly RegisterUseCase _registerUseCase;
    private readonly INavigationService _navigationService;
    private readonly IInfoBarService _infoBarService;
    public RegisterViewModel(
        RegisterUseCase registerUseCase,
        INavigationService navigationService,
        IInfoBarService infoBarService)
    {
        _registerUseCase = registerUseCase;
        _navigationService = navigationService;
        _infoBarService = infoBarService;
    }

    [RelayCommand]
    public async Task RegisterAsync()
    { 
        if (_password != _confirmPassword)
        {
            _infoBarService.ShowError("Passwords do not match");
            return;
        }

        var request = new RegisterRequestDto(
            Email: _email,
            Password: _password,
            Username: _username,
            FullName: _fullName
            );

        var result = await _registerUseCase.ExecuteAsync(request);
        if (result.IsFailure)
        {
            _infoBarService.ShowError(result.Error);
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Success",
            Content = "Registration sucessfully",
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();

        await _navigationService.NavigateToAsync<LoginViewModel>(parameter: _email, clearNavigation: true);
    }

    [RelayCommand]
    public async Task NavigateToLoginAsync()
    {
        await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
