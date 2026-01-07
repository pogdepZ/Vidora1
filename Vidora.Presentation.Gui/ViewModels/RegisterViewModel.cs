using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Vidora.Core.Exceptions;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class RegisterViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;


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
    public async Task ExecuteRegisterAsync()
    { 
        if (Password != ConfirmPassword)
        {
            await _infoBarService.ShowErrorAsync("Passwords do not match");
            return;
        }
        
        try
        {
            var request = new Core.Contracts.Commands.RegisterCommand(
                Email: Email,
                Password: Password,
                Username: Username,
                FullName: FullName
                );

            var result = await _registerUseCase.ExecuteAsync(request);
        }
        catch (DomainException ex)
        {
            await _infoBarService.ShowErrorAsync(ex.Message, durationMs: 2000);
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

        await _navigationService.NavigateToAsync<LoginViewModel>(parameter: Email, clearNavigation: true);
    }

    [RelayCommand]
    public async Task ExecuteNavigateToLoginAsync()
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
