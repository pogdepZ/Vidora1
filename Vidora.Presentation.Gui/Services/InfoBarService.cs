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

    public Task ShowSuccessAsync(string message, int durationMs = 1500)
        => ShowAsync(message, InfoBarSeverity.Success, durationMs);

    public Task ShowErrorAsync(string message, int durationMs = 1500)
        => ShowAsync(message, InfoBarSeverity.Error, durationMs);

    public Task ShowWarningAsync(string message, int durationMs = 1500)
        => ShowAsync(message, InfoBarSeverity.Warning, durationMs);

    public Task ShowInfoAsync(string message, int durationMs = 1500)
        => ShowAsync(message, InfoBarSeverity.Informational, durationMs);

    public async Task CloseAsync()
    {
        if (_infoBar is null || !_infoBar.IsOpen)
            return;

        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = null;

        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            _infoBar.IsOpen = false;
            _infoBar.Opacity = 1;
        });
    }

    private async Task ShowAsync(
            string message,
            InfoBarSeverity severity,
            int durationMs)
    {
        if (_infoBar is null)
            throw new InvalidOperationException("InfoBar not initialized.");

        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = new CancellationTokenSource();
        var token = _currentCts.Token;

        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            _infoBar.Message = message;
            _infoBar.Severity = severity;
            _infoBar.Opacity = 1;
            _infoBar.IsOpen = true;
        });

        if (durationMs <= 0)
            return;

        try
        {
            await Task.Delay(durationMs, token);
            await FadeOutAsync(_infoBar, 200, token);

            if (!token.IsCancellationRequested)
            {
                await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
                {
                    _infoBar.IsOpen = false;
                    _infoBar.Opacity = 1;
                });
            }
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }

    private static async Task FadeOutAsync(
        InfoBar infoBar,
        int durationMs,
        CancellationToken token)
    {
        const double frameMs = 16.67; // ~60fps
        int steps = Math.Max(1, durationMs / (int)frameMs);

        for (int i = 0; i < steps; i++)
        {
            if (token.IsCancellationRequested)
                return;

            double progress = (double)i / steps;
            infoBar.Opacity = 1.0 - progress;

            await Task.Delay((int)frameMs, token);
        }

        infoBar.Opacity = 0;
    }
}
