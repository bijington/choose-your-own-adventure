using Microsoft.Maui.Graphics;
using ChooseYourOwnAdventure.Presentation;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class ChampionGameObject : GameObject
{
    private readonly AdventureState _state;
    private readonly bool _onBookPage;
    private readonly bool _chapterPage;

    public ChampionGameObject(AdventureState state, bool onBookPage = false, bool chapterPage = false)
    {
        _state = state;
        _onBookPage = onBookPage;
        _chapterPage = chapterPage;
    }

    public string ChampionName => GetChampionName(_state.Champion);

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        if (_state.OpeningStage is not (OpeningStage.ChampionChosen or OpeningStage.ChampionTransition or OpeningStage.CodeReveal or OpeningStage.Complete))
        {
            return;
        }

        var rect = GetRenderPanel(dimensions);
        if (!_chapterPage) DrawPanel(canvas, rect);
        DrawChampion(canvas, rect, _state.Champion, showLabels: !_chapterPage);
    }

    private RectF GetRenderPanel(RectF dimensions)
    {
        if (_chapterPage)
        {
            var right = BookLayout.RightPage(dimensions);
            return new RectF(right.X, dimensions.Y + dimensions.Height * .39f,
                right.Width, dimensions.Height * .43f);
        }
        if (_onBookPage) return BookLayout.RightPage(dimensions);

        var finalRect = GetSpritePanel(dimensions);
        if (_state.OpeningStage is OpeningStage.CodeReveal or OpeningStage.Complete)
        {
            return finalRect;
        }

        var startRect = GetChampionStagePanel(dimensions);
        if (_state.OpeningStage == OpeningStage.ChampionChosen)
        {
            return startRect;
        }

        var t = EaseInOut(_state.CodeRevealTransitionProgress);
        return Lerp(startRect, finalRect, t);
    }

    private static RectF GetChampionStagePanel(RectF dimensions)
    {
        var width = Math.Clamp(dimensions.Width * 0.36f, 330f, 520f);
        var height = Math.Clamp(dimensions.Height * 0.50f, 300f, 500f);
        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.28f,
            width,
            height);
    }

    private static RectF GetSpritePanel(RectF dimensions)
    {
        var margin = Math.Max(42f, dimensions.Width * 0.05f);
        var gap = Math.Max(28f, dimensions.Width * 0.035f);
        var contentTop = dimensions.Y + dimensions.Height * 0.16f;
        var contentHeight = dimensions.Height * 0.66f;
        var panelWidth = (dimensions.Width - (margin * 2) - gap) / 2f;
        var left = new RectF(dimensions.X + margin, contentTop, panelWidth, contentHeight);

        return new RectF(left.Right + gap, contentTop, panelWidth, contentHeight);
    }

    private static RectF Lerp(RectF from, RectF to, float t) => new(
        from.X + (to.X - from.X) * t,
        from.Y + (to.Y - from.Y) * t,
        from.Width + (to.Width - from.Width) * t,
        from.Height + (to.Height - from.Height) * t);

    private static float EaseInOut(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return value * value * (3f - 2f * value);
    }

    private static void DrawPanel(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);
    }

    private static void DrawChampion(ICanvas canvas, RectF rect, Champion? champion, bool showLabels = true)
    {
        if (showLabels)
        {
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Colors.White;
            canvas.FontSize = Math.Clamp(rect.Width * 0.064f, 18f, 30f);
            canvas.DrawString(
                GetChampionName(champion),
                rect.X + 20,
                rect.Y + 24,
                rect.Width - 40,
                40,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);

        }

        var centerX = rect.X + rect.Width / 2f;
        var groundY = rect.Y + rect.Height * (showLabels ? .75f : .60f);
        var scale = showLabels
            ? Math.Clamp(rect.Width * .045f, 10f, 18f)
            : Math.Clamp(rect.Width * .065f, 16f, 28f);

        switch (champion)
        {
            case Champion.Charioteer:
                DrawCharioteer(canvas, centerX, groundY, scale);
                break;
            case Champion.SpacePilot:
                DrawSpacePilot(canvas, centerX, groundY, scale);
                break;
            default:
                DrawRacingDriver(canvas, centerX, groundY, scale);
                break;
        }

        if (!showLabels) return;

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(rect.Width * 0.038f, 12f, 18f);
        canvas.DrawString(
            "Rendered by this Orbit GameObject",
            rect.X + 20,
            rect.Bottom - 58,
            rect.Width - 40,
            32,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }

    private static void DrawRacingDriver(ICanvas canvas, float x, float y, float p)
    {
        Pixel(canvas, x, y, p, -4.5f, -2.5f, 9f, 2.8f, Color.FromArgb("#EF4444"));
        Pixel(canvas, x, y, p, -2.5f, -4.2f, 5f, 2f, Color.FromArgb("#F8FAFC"));
        Pixel(canvas, x, y, p, -1.7f, -3.8f, 3.4f, 1.2f, Color.FromArgb("#38BDF8"));
        Pixel(canvas, x, y, p, -3.8f, -0.4f, 2f, 1.5f, Color.FromArgb("#0F172A"));
        Pixel(canvas, x, y, p, 1.8f, -0.4f, 2f, 1.5f, Color.FromArgb("#0F172A"));
        Pixel(canvas, x, y, p, -0.6f, -5.7f, 1.2f, 1.5f, Color.FromArgb("#F8D866"));
    }

    private static void DrawCharioteer(ICanvas canvas, float x, float y, float p)
    {
        Pixel(canvas, x, y, p, -4.5f, -2.1f, 5.8f, 2.1f, Color.FromArgb("#D97706"));
        Pixel(canvas, x, y, p, -3.8f, -4.6f, 1.7f, 2.6f, Color.FromArgb("#F1C7A5"));
        Pixel(canvas, x, y, p, -4.1f, -5.5f, 2.3f, 1f, Color.FromArgb("#F8D866"));
        Pixel(canvas, x, y, p, 1.0f, -2.7f, 4.7f, 1.7f, Color.FromArgb("#A16207"));
        Pixel(canvas, x, y, p, 4.8f, -3.6f, 1.1f, 1.3f, Color.FromArgb("#A16207"));
        Pixel(canvas, x, y, p, -3.6f, -0.4f, 1.8f, 1.8f, Color.FromArgb("#1E293B"));
        Pixel(canvas, x, y, p, -0.5f, -0.4f, 1.8f, 1.8f, Color.FromArgb("#1E293B"));
    }

    private static void DrawSpacePilot(ICanvas canvas, float x, float y, float p)
    {
        Pixel(canvas, x, y, p, -4.5f, -2.2f, 9f, 2.2f, Color.FromArgb("#6366F1"));
        Pixel(canvas, x, y, p, -2.6f, -4.1f, 5.2f, 2.2f, Color.FromArgb("#CBD5E1"));
        Pixel(canvas, x, y, p, -1.8f, -3.7f, 3.6f, 1.2f, Color.FromArgb("#22D3EE"));
        Pixel(canvas, x, y, p, -6f, -1.7f, 2.1f, 1.2f, Color.FromArgb("#A78BFA"));
        Pixel(canvas, x, y, p, 3.9f, -1.7f, 2.1f, 1.2f, Color.FromArgb("#A78BFA"));
        Pixel(canvas, x, y, p, -1.8f, 0f, 1.2f, 1.8f, Color.FromArgb("#F97316"));
        Pixel(canvas, x, y, p, 0.6f, 0f, 1.2f, 1.8f, Color.FromArgb("#F97316"));
    }

    private static void Pixel(ICanvas canvas, float centerX, float groundY, float p, float x, float y, float w, float h, Color color)
    {
        canvas.FillColor = color;
        canvas.FillRectangle(centerX + x * p, groundY + y * p, w * p, h * p);
    }

    private static string GetChampionName(Champion? champion) => champion switch
    {
        Champion.Charioteer => "The Charioteer",
        Champion.SpacePilot => "The Space Pilot",
        _ => "The Racing Driver"
    };
}
