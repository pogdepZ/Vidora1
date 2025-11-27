using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.Services;

namespace Vidora.Presentation.Gui.Services;

public class InfoBarService : IInfoBarService
{
    private InfoBar? _infoBar;
    private CancellationTokenSource? _currentCts;

    [MemberNotNull(nameof(_infoBar))]
    public void Initialize(InfoBar infoBar)
    {
        _infoBar = infoBar;
    }

    public void CloseIfOpen()
    {
        if (_infoBar is null || !_infoBar.IsLoaded || !_infoBar.IsOpen)
            return;

        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = null;

        _infoBar.IsOpen = false;
    }

    public void ShowSuccess(string message, int durationMs = 1500)
        => Show(message, InfoBarSeverity.Success, durationMs);

    public void ShowError(string message, int durationMs = 1500)
        => Show(message, InfoBarSeverity.Error, durationMs);

    public void ShowWarning(string message, int durationMs = 1500)
        => Show(message, InfoBarSeverity.Warning, durationMs);

    public void ShowInfo(string message, int durationMs = 1500)
        => Show(message, InfoBarSeverity.Informational, durationMs);

    private void Show(string message, InfoBarSeverity severity, int durationMs)
    {
        if (_infoBar is null)
            throw new InvalidOperationException("InfoBar not initialized.");

        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = new CancellationTokenSource();
        var token = _currentCts.Token;

        _infoBar.Message = message;
        _infoBar.Severity = severity;
        _infoBar.IsOpen = true;

        CloseAfterDelayAsync(token, durationMs);
    }

    private async void CloseAfterDelayAsync(CancellationToken token, int durationMs)
    {
        try
        {
            await Task.Delay(durationMs, token);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"InfoBar error: {ex}");
        }
        finally
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                await FadeOutAsync(_infoBar!, 200, token);

                if (!token.IsCancellationRequested)
                    _infoBar!.IsOpen = false;

                _infoBar!.Opacity = 1;
            });
        }
    }

    private async Task FadeOutAsync(InfoBar infoBar, int durationMs, CancellationToken token)
    {
        const double frame = 16.67; // ~60fps
        int steps = durationMs / (int)frame;

        for (int i = 0; i < steps; i++)
        {
            if (token.IsCancellationRequested)
                return;

            double progress = (double)i / steps;
            infoBar.Opacity = 1.0 - progress;

            await Task.Delay((int)frame);
        }

        infoBar.Opacity = 0;
    }
}
