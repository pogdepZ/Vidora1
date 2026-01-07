using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Vidora.Core.Exceptions;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private string _versionDescription = string.Empty;

    [ObservableProperty]
    private ElementTheme _selectedTheme;

    public List<ElementTheme> ElementThemes { get; }

    private readonly LogoutUseCase _logoutUseCase;
    private readonly IInfoBarService _infoBarService;
    private readonly IThemeSelectorService _themeSelectorService;
    public SettingsViewModel(
        LogoutUseCase logoutUseCase,
        IInfoBarService infoBarService,
        IThemeSelectorService themeSelectorService)
    {
        _logoutUseCase = logoutUseCase;
        _infoBarService = infoBarService;
        _themeSelectorService = themeSelectorService;
        
        SelectedTheme = _themeSelectorService.Theme;

        ElementThemes =
        [
            ElementTheme.Light,
            ElementTheme.Dark,
            ElementTheme.Default
        ];

        VersionDescription = GetVersionDescription();
    }

    [RelayCommand]
    public async Task ExecuteLogoutAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Confirm Logout",
            Content = "Are you sure you want to log out?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot,
            RequestedTheme = SelectedTheme
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            await _logoutUseCase.ExecuteAsync();
        }
        catch (DomainException ex)
        {
            await _infoBarService.ShowErrorAsync(ex.Message);
        }
    }

    public async Task OnNavigatedToAsync(object? param)
    {

    }
    public async Task OnNavigatedFromAsync()
    {

    }

    private static string GetVersionDescription()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        if (version == null)
        {
            return "1.0.0.0";
        }

        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    partial void OnSelectedThemeChanged(ElementTheme value)
    {
        _themeSelectorService.SetTheme(value);
    }
}
