using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class HeroBannerControl : UserControl
{
    private CanvasBitmap? _bitmap;

    public HeroBannerControl()
    {
        InitializeComponent();

        Loaded += (_, __) => TryLoadImageAsync();
        BannerCanvas.Draw += OnDraw;
        BannerHost.SizeChanged += (_, __) => UpdateCanvasHeight();
        BannerCanvas.SizeChanged += (_, __) => BannerCanvas.Invalidate();
    }

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(HeroBannerControl),
            new PropertyMetadata(null, OnSourceChanged));

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeroBannerControl view && e.NewValue is string s)
            _ = view.LoadImageAsync(s);
    }

    private async void TryLoadImageAsync()
    {
        if (!string.IsNullOrWhiteSpace(Source))
            await LoadImageAsync(Source!);

        BannerCanvas.Invalidate();
    }

    private async Task LoadImageAsync(string src)
    {
        try
        {
            using var stream = await OpenStreamAsync(src);
            if (stream == null)
                return;

            _bitmap = await CanvasBitmap.LoadAsync(
                BannerCanvas.Device,
                stream.AsRandomAccessStream());

            UpdateCanvasHeight();
            BannerCanvas.Invalidate();
        }
        catch
        {
            _bitmap = null;
        }
    }

    private static async Task<Stream?> OpenStreamAsync(string src)
    {
        if (!Uri.TryCreate(src, UriKind.RelativeOrAbsolute, out var uri))
            return null;

        // convert "Assets/..." -> ms-appx:///
        if (!uri.IsAbsoluteUri)
            uri = new Uri("ms-appx:///" + src.TrimStart('/'));

        return uri.Scheme switch
        {
            "http" or "https" => new MemoryStream(await new HttpClient().GetByteArrayAsync(uri)),
            "ms-appx" => (await StorageFile.GetFileFromApplicationUriAsync(uri)).OpenStreamForReadAsync().Result,
            _ when uri.IsFile => File.OpenRead(uri.LocalPath),
            _ => null
        };
    }

    private void UpdateCanvasHeight()
    {
        if (_bitmap == null) return;

        double width = BannerHost.ActualWidth;
        if (width <= 0) return;

        double ratio = GetImageRatioSafely(_bitmap);
        BannerHost.Height = width * ratio;
    }

    private static double GetImageRatioSafely(CanvasBitmap bmp)
    {
        try
        {
            var s = bmp.Size;
            return (s.Width > 0) ? s.Height / s.Width : bmp.SizeInPixels.Height / (double)bmp.SizeInPixels.Width;
        }
        catch
        {
            var px = bmp.SizeInPixels;
            return px.Height / (double)px.Width;
        }
    }

    private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (_bitmap == null) return;

        float w = (float)sender.ActualWidth;
        float h = (float)sender.ActualHeight;
        if (w <= 0 || h <= 0) return;

        var ds = args.DrawingSession;

        var imageLayer = DrawImageLayer(sender, w, h);
        var maskH = CreateHorizontalMask(sender, w, h);
        var maskV = CreateVerticalMask(sender, w, h);

        var finalMask = new CompositeEffect
        {
            Mode = CanvasComposite.DestinationIn,
            Sources = { maskH, maskV }
        };

        var final = new CompositeEffect
        {
            Mode = CanvasComposite.DestinationIn,
            Sources = { imageLayer, finalMask }
        };

        ds.DrawImage(final);
    }

    private CanvasCommandList DrawImageLayer(CanvasControl canvas, float w, float h)
    {
        var cmd = new CanvasCommandList(canvas);
        using var ds = cmd.CreateDrawingSession();
        ds.DrawImage(_bitmap, new Rect(0, 0, w, h));
        return cmd;
    }

    private static CanvasCommandList CreateHorizontalMask(CanvasControl canvas, float w, float h)
    {
        var cmd = new CanvasCommandList(canvas);
        using var ds = cmd.CreateDrawingSession();

        var brush = new CanvasLinearGradientBrush(canvas, new[]
        {
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(0,0,0,0), Position = 0f },
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(255,0,0,0), Position = 0.2f },
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(255,0,0,0), Position = 0.5f },
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(0,0,0,0), Position = 1f },
        })
        {
            StartPoint = new Vector2(w, 0),
            EndPoint = new Vector2(0, 0)
        };

        ds.FillRectangle(0, 0, w, h, brush);
        return cmd;
    }

    private static CanvasCommandList CreateVerticalMask(CanvasControl canvas, float w, float h)
    {
        var cmd = new CanvasCommandList(canvas);
        using var ds = cmd.CreateDrawingSession();

        var brush = new CanvasLinearGradientBrush(canvas, new[]
        {
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(0,0,0,0), Position = 0f },
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(255,0,0,0), Position = 0.5f },
            new CanvasGradientStop { Color = Windows.UI.Color.FromArgb(0,0,0,0), Position = 1f },
        })
        {
            StartPoint = new Vector2(0, 0),
            EndPoint = new Vector2(0, h)
        };

        ds.FillRectangle(0, 0, w, h, brush);
        return cmd;
    }
}
