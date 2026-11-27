using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class GameLoopScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public GameLoopScene(AdventureState state)
    {
        _state = state;
        Add(new LoopChampionMarker(state));
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "A FRAME IN THE LIFE\nOF OUR CHAMPION",
            "The loop is running. Let’s follow one frame with our champion.");
        DrawStory(canvas, dimensions);
        DrawLoop(canvas, dimensions);
        base.Render(canvas, dimensions);
        DrawPromptAndNext(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.GameLoop, _state.Champion));
    }

    private void DrawStory(ICanvas canvas, RectF dimensions)
    {
        var left = BookLayout.LeftPage(dimensions);
        var beat = ChampionLoopStory.For(_state.Champion, _state.GameLoopStation);
        DrawText(canvas, $"{(int)_state.GameLoopStation + 1} / 4 · {_state.GameLoopStation.ToString().ToUpperInvariant()}",
            new RectF(left.X, left.Y, left.Width, dimensions.Height * .055f), 21, true);
        DrawText(canvas, $"“{beat.Thought}”", new RectF(left.X, dimensions.Y + dimensions.Height * .33f,
            left.Width, dimensions.Height * .15f), 28, true);
        DrawText(canvas, beat.Explanation, new RectF(left.X, dimensions.Y + dimensions.Height * .51f,
            left.Width, dimensions.Height * .23f), 21);
        DrawText(canvas, beat.Takeaway, new RectF(left.X, dimensions.Y + dimensions.Height * .78f,
            left.Width, dimensions.Height * .06f), 21, true);
    }

    private void DrawLoop(ICanvas canvas, RectF dimensions)
    {
        var track = LoopLayout.GetTrackBounds(dimensions);
        canvas.StrokeColor = PresentationTheme.TrackSurface;
        canvas.StrokeSize = 28;
        canvas.DrawEllipse(track);
        canvas.StrokeColor = PresentationTheme.TrackEdge;
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track);
        foreach (var station in Enum.GetValues<LoopStation>())
        {
            var position = LoopLayout.GetStationPosition(track, station);
            var active = station == _state.GameLoopStation;
            var radius = active ? 31f : 27f;
            canvas.FillColor = active ? PresentationTheme.CodePanelHeader : PresentationTheme.CodePanel;
            canvas.FillEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);
            canvas.StrokeColor = active ? Accent : PresentationTheme.TrackEdge;
            canvas.StrokeSize = active ? 3 : 1.5f;
            canvas.DrawEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = active ? Accent : Ink;
            canvas.FontSize = 13;
            canvas.DrawString(station.ToString().ToUpperInvariant(), position.X - 54, position.Y - 12, 108, 24,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        DrawText(canvas, "ONE FRAME.\nTHEN ANOTHER.", new RectF(track.X + 35, track.Y + track.Height * .32f,
            track.Width - 70, track.Height * .36f), 20, true, HorizontalAlignment.Center);
    }

    private static void DrawText(ICanvas canvas, string text, RectF bounds, float size,
        bool accent = false, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        canvas.Font = accent ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = accent ? Accent : Ink;
        canvas.FontSize = size;
        canvas.DrawString(text, bounds, alignment, VerticalAlignment.Center);
    }

    private void DrawPromptAndNext(ICanvas canvas, RectF dimensions)
    {
        DrawPrompt(canvas, dimensions, "SOUTH / ENTER · NEXT STATION    EAST / LEFT · PREVIOUS");
        var right = BookLayout.RightPage(dimensions);
        var bounds = new RectF(right.Right - 220, dimensions.Bottom - 94, 220, 38);
        canvas.FillColor = PresentationTheme.CodePanelHeader;
        canvas.FillRoundedRectangle(bounds, 12);
        canvas.StrokeColor = Accent;
        canvas.StrokeSize = 1.5f;
        canvas.DrawRoundedRectangle(bounds, 12);
        DrawText(canvas, "RB / PGDN · TAKE CONTROL →", bounds, 12, true, HorizontalAlignment.Center);
    }
}
