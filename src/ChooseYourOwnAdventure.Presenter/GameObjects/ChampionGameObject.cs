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
            return new RectF(right.X, dimensions.Y + dimensions.Height * .46f,
                right.Width, dimensions.Height * .38f);
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
        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);
    }

    private static void DrawChampion(ICanvas canvas, RectF rect, Champion? champion, bool showLabels = true)
    {
        if (showLabels)
        {
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = PresentationTheme.Ink;
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

        var artwork = showLabels
            ? new RectF(rect.X + 24f, rect.Y + 78f, rect.Width - 48f, rect.Height - 154f)
            : new RectF(rect.X + 16f, rect.Y + 8f, rect.Width - 32f, rect.Height - 16f);
        ChampionSprites.DrawCharacter(canvas, champion ?? Champion.RacingDriver, artwork);

        if (!showLabels) return;

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
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

    private static string GetChampionName(Champion? champion) => champion switch
    {
        Champion.Charioteer => "The Charioteer",
        Champion.SpacePilot => "The Space Pilot",
        _ => "The Racing Driver"
    };
}
