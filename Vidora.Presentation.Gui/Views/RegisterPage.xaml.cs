using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; } = App.GetService<RegisterViewModel>();
    public RegisterPage()
    {
        InitializeComponent();
    }

    [GeneratedRegex(@"[a-zA-Z0-9@._\-+]", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"[a-zA-Z0-9_]", RegexOptions.Compiled)]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(@"[a-zA-Z\s]", RegexOptions.Compiled)]
    private static partial Regex FullNameRegex();

    private void EmailTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (sender is not TextBox textBox) return;

        var regex = EmailRegex();

        if (args.NewText.Any(c => !regex.IsMatch(c.ToString())))
        {
            args.Cancel = true;
        }
    }

    private void UsernameTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        var regex = UsernameRegex();

        if (args.NewText.Any(c => !regex.IsMatch(c.ToString())))
        {
            args.Cancel = true;
        }
    }

    private void FullNameTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        var regex = FullNameRegex();

        if (args.NewText.Any(c => !regex.IsMatch(c.ToString())))
        {
            args.Cancel = true;
        }
    }
}
