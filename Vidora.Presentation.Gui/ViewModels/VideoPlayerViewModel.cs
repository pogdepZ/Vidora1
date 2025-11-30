using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;
using Windows.Media.Core;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class VideoPlayerViewModel : ObservableRecipient, INavigationAware
{
    // TODO: add PosterSource

    private MediaSource? _MediaSource;
    public MediaSource? MediaSource
    {
        get => _MediaSource;
        set => SetProperty(ref _MediaSource, value);
    }

    public VideoPlayerViewModel()
    {
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        if (parameter is MediaSource directSource)
        {
            MediaSource = directSource;
            return;
        }

        if (parameter is string url && !string.IsNullOrWhiteSpace(url))
        {
            MediaSource = CreateMediaSourceFromString(url);
        }
    }

    public async Task OnNavigatedFromAsync()
    {
        if (MediaSource is not null)
        {
            // MediaSource has no Dispose, but clear ref so player can release
            MediaSource = null;
        }
    }

    private static MediaSource? CreateMediaSourceFromString(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
            return null;

        src = src.Trim();

        if (!Uri.TryCreate(src, UriKind.Absolute, out var uri))
            return null;

        if (uri.Scheme is not ("http" or "https" or "ms-appx" or "ms-appdata"))
            return null;

        try
        {
            return MediaSource.CreateFromUri(uri);
        }
        catch
        {
            return null;
        }
    }
}
