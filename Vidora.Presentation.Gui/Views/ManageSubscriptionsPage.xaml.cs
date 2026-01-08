using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageSubscriptionsPage : Page
{
    public ManageSubscriptionsViewModel ViewModel { get; } = App.GetService<ManageSubscriptionsViewModel>();

    public ManageSubscriptionsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void OnAddPromoClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetAddPromoForm();
        AddPromoDialog.XamlRoot = this.XamlRoot;
        await AddPromoDialog.ShowAsync();
    }

    private async void OnAddPromoPrimaryClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        try
        {
            sender.IsPrimaryButtonEnabled = false;
            sender.IsSecondaryButtonEnabled = false;

            var success = await ViewModel.TryCreatePromoAsync();
            if (success)
            {
                args.Cancel = false;
            }
        }
        finally
        {
            sender.IsPrimaryButtonEnabled = true;
            sender.IsSecondaryButtonEnabled = true;
        }
    }

    private async void OnOrderSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await ViewModel.ApplyOrderFiltersCommand.ExecuteAsync(null);
    }

    private async void OnPromoSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await ViewModel.ApplyPromoFiltersCommand.ExecuteAsync(null);
    }

    private async void OnOrderFilterChanged(object sender, SelectionChangedEventArgs e)
        => await ViewModel.ApplyOrderFiltersCommand.ExecuteAsync(null);

    private async void OnPromoFilterChanged(object sender, SelectionChangedEventArgs e)
        => await ViewModel.ApplyPromoFiltersCommand.ExecuteAsync(null);
}
