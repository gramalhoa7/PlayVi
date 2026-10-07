using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace PlayViApp;

public partial class PlayerPage : ContentPage
{
    private bool _isDragging;
    private CancellationTokenSource? _hideCts;

    public PlayerPage(string title, string url)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        Player.Source = MediaSource.FromUri(url);
    }

    // ---------- Botões ----------

    private void OnPlayPauseClicked(object? sender, EventArgs e)
    {
        if (Player.CurrentState == MediaElementState.Playing) Player.Pause();
        else Player.Play();
        ShowControls();
    }

    private async void OnSkipBackClicked(object? sender, EventArgs e) => await SeekByAsync(-10);
    private async void OnSkipForwardClicked(object? sender, EventArgs e) => await SeekByAsync(10);

    private async Task SeekByAsync(int seconds)
    {
        var max = Player.Duration.TotalSeconds;
        var target = Player.Position.TotalSeconds + seconds;
        target = Math.Max(0, max > 0 ? Math.Min(target, max) : target);

        await Player.SeekTo(TimeSpan.FromSeconds(target));
        ShowControls();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        Player.Stop();
        await Navigation.PopModalAsync();
    }

    // ---------- Barra de progresso ----------

    private void OnDragStarted(object? sender, EventArgs e)
    {
        _isDragging = true;
        _hideCts?.Cancel();
    }

    private async void OnDragCompleted(object? sender, EventArgs e)
    {
        await Player.SeekTo(TimeSpan.FromSeconds(ProgressSlider.Value));
        _isDragging = false;
        ShowControls();
    }

    private void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        // Enquanto o dedo/mouse arrasta, mostra o tempo de destino.
        if (_isDragging) TimeLabel.Text = Format(TimeSpan.FromSeconds(e.NewValue));
    }

    // ---------- Eventos do player ----------

    private void OnMediaOpened(object? sender, EventArgs e) => Dispatcher.Dispatch(() =>
    {
        var total = Player.Duration.TotalSeconds;
        if (total > 0) ProgressSlider.Maximum = total;
        DurationLabel.Text = Format(Player.Duration);
    });

    private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e) => Dispatcher.Dispatch(() =>
    {
        if (_isDragging) return;
        ProgressSlider.Value = Math.Min(e.Position.TotalSeconds, ProgressSlider.Maximum);
        TimeLabel.Text = Format(e.Position);
    });

    private void OnStateChanged(object? sender, MediaStateChangedEventArgs e) => Dispatcher.Dispatch(() =>
    {
        Spinner.IsVisible = e.NewState is MediaElementState.Opening or MediaElementState.Buffering;
        PlayPauseButton.Text = e.NewState == MediaElementState.Playing ? "❚❚" : "▶";

        if (e.NewState == MediaElementState.Playing) ScheduleHide();
        else ShowControls();
    });

    private void OnMediaEnded(object? sender, EventArgs e) => Dispatcher.Dispatch(() =>
    {
        PlayPauseButton.Text = "▶";
        ShowControls();
    });

    private void OnMediaFailed(object? sender, MediaFailedEventArgs e) => Dispatcher.Dispatch(() =>
    {
        Spinner.IsVisible = false;
        ErrorLabel.Text = "Não foi possível reproduzir este vídeo.";
        ErrorLabel.IsVisible = true;
        ShowControls();
    });

    // ---------- Mostrar e esconder os controles ----------

    private void OnScreenTapped(object? sender, TappedEventArgs e)
    {
        if (Controls.IsVisible && Controls.Opacity > 0) HideNow();
        else ShowControls();
    }

    private void ShowControls()
    {
        _hideCts?.Cancel();
        Controls.CancelAnimations();
        Controls.Opacity = 1;
        Controls.IsVisible = true;

        if (Player.CurrentState == MediaElementState.Playing && !_isDragging)
            ScheduleHide();
    }

    private void HideNow()
    {
        _hideCts?.Cancel();
        Controls.CancelAnimations();
        Controls.IsVisible = false;
    }

    private async void ScheduleHide()
    {
        _hideCts?.Cancel();
        _hideCts = new CancellationTokenSource();
        var token = _hideCts.Token;

        try
        {
            await Task.Delay(3000, token);
            await Controls.FadeTo(0, 250);
            if (!token.IsCancellationRequested) Controls.IsVisible = false;
        }
        catch (TaskCanceledException) { }
    }

    // ---------- Utilidades ----------

    private static string Format(TimeSpan t) =>
        t.TotalHours >= 1
            ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}"
            : $"{t.Minutes:00}:{t.Seconds:00}";

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _hideCts?.Cancel();
        Player.Stop();
        Player.Handler?.DisconnectHandler();
    }
}