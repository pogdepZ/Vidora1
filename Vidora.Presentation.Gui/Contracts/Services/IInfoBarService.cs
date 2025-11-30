using Microsoft.UI.Xaml.Controls;

namespace Vidora.Presentation.Gui.Contracts.Services;

public interface IInfoBarService
{
    void Initialize(InfoBar infoBar);
    void CloseIfOpen();
    void ShowInfo(string message, int durationMs = 1500);
    void ShowSuccess(string message, int durationMs = 1500);
    void ShowWarning(string message, int durationMs = 1500);
    void ShowError(string message, int durationMs = 1500);
}
