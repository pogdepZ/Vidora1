using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Windows.Media.Core;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class VideoPlayerViewModel : ObservableRecipient, INavigationAware
{
    // TODO: add PosterSource
    [ObservableProperty]
    private MediaSource? _MediaSource;

    [ObservableProperty]
    private string _title = string.Empty;

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
            return;
        }

        if (parameter != null)
        {
            var paramType = parameter.GetType();
            var urlProp = paramType.GetProperty("Url");
            var titleProp = paramType.GetProperty("Title");

            if (urlProp != null && urlProp.GetValue(parameter) is string urlValue)
            {
                MediaSource = CreateMediaSourceFromString(urlValue);
            }

            if (titleProp != null && titleProp.GetValue(parameter) is string titleValue)
            {
                Title = titleValue;
            }
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
