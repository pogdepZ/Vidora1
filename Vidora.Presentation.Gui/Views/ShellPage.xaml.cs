using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Helpers;
using Vidora.Presentation.Gui.ViewModels;
using Windows.Foundation;
using Windows.System;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ShellPage : Page
{
    ShellViewModel ViewModel { get; } = App.GetService<ShellViewModel>();
    public ShellPage()
    {
        InitializeComponent();
        ViewModel.InfoBarService.Initialize(AppInfoBar);
        ViewModel.NavigationViewService.Initialize(AppNavigationView);
        
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        RegisterBackKeyboardAccelerators();
    }

    private void RegisterBackKeyboardAccelerators()
    {
        var backAlt = BuildKeyboardAccelerator(VirtualKey.Left, OnBackRequested, modifiers: VirtualKeyModifiers.Menu);
        KeyboardAccelerators.Add(backAlt);

        var backKey = BuildKeyboardAccelerator(VirtualKey.GoBack, OnBackRequested);
        KeyboardAccelerators.Add(backKey);
    }

    private KeyboardAccelerator BuildKeyboardAccelerator(
        VirtualKey key,
        TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> handler,
        VirtualKeyModifiers? modifiers = null
        )
    {
        var accelerator = new KeyboardAccelerator { Key = key };

        if (modifiers.HasValue)
        {
            accelerator.Modifiers = modifiers.Value;
        }

        accelerator.Invoked += handler;
        return accelerator;
    }

    private async void OnBackRequested(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = await ViewModel.NavigationService.GoBackAsync();
    }
}
