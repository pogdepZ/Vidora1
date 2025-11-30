using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private string _versionDescription = string.Empty;
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    private ElementTheme _selectedTheme;
    public ElementTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeSelectorService.SetTheme(value);
            }
        }
    }

    public List<ElementTheme> ElementThemes { get; }

    private readonly LogoutUseCase _logoutUseCase;
    private readonly IThemeSelectorService _themeSelectorService;
    public SettingsViewModel(LogoutUseCase logoutUseCase, IThemeSelectorService themeSelectorService)
    {
        _logoutUseCase = logoutUseCase;
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
    public async Task LogoutAsync()
    {
        // TODO: add dialog confirmation

        await _logoutUseCase.ExecuteAsync();
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
}
