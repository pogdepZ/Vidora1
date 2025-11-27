using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using Vidora.Presentation.Gui.ViewModels;


namespace Vidora.Presentation.Gui.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; } = App.GetService<LoginViewModel>();
    public LoginPage()
    {
        InitializeComponent();
    }

    [GeneratedRegex(@"[a-zA-Z0-9@._\-+]", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    private void EmailTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (sender is not TextBox textBox) return;

        var regex = EmailRegex();

        if (args.NewText.Any(c => !regex.IsMatch(c.ToString())))
        {
            args.Cancel = true;
        }
    }
}
