using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Contracts.Services;

public interface IInfoBarService
{
    void Initialize(InfoBar infoBar);

    Task ShowSuccessAsync(string message, int durationMs = 1500);
    Task ShowErrorAsync(string message, int durationMs = 1500);
    Task ShowWarningAsync(string message, int durationMs = 1500);
    Task ShowInfoAsync(string message, int durationMs = 1500);

    Task CloseAsync();
}
