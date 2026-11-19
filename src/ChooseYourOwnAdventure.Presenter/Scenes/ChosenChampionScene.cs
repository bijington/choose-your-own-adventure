using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

/// <summary>Reveal the chosen character, then its rendering code on the facing leaf.</summary>
public sealed class ChosenChampionScene : ChapterOpeningScene
{
    private readonly AdventureState _state;
    private float _codeOpacity;

    public ChosenChampionScene(AdventureState state) : base(state,
        PresentationSlide.ChosenChampion, "THE AUDIENCE HAS CHOSEN", "OUR CHAMPION",
        string.Empty, showChampion: true)
    {
        _state = state;
    }

    protected override string ContinuePrompt => _state.ChampionRenderingVisible
        ? "SOUTH / ENTER · HOW DOES THEIR WORLD STAY ALIVE?"
        : "SOUTH / ENTER · HOW IS OUR CHAMPION DRAWN?";

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _codeOpacity = _state.ChampionRenderingVisible
            ? Math.Min(1f, _codeOpacity + (float)(millisecondsSinceLastUpdate / 350d))
            : 0f;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);
        if (!_state.ChampionRenderingVisible) return;

        var left = BookLayout.LeftPage(dimensions);
        canvas.SaveState();
        canvas.Alpha = _codeOpacity;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .025f, 23f, 36f);
        canvas.DrawString("HOW OUR CHAMPION IS DRAWN", left.X, dimensions.Y + dimensions.Height * .08f,
            left.Width, dimensions.Height * .12f, HorizontalAlignment.Left, VerticalAlignment.Center);

        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .60f),
            "ChampionGameObject.cs · Render()",
            """
public override void Render(
    ICanvas canvas, RectF dimensions)
{
    base.Render(canvas, dimensions);

    var panel = GetRenderPanel(dimensions);
    DrawChampion(canvas, panel,
        _state.Champion);
}
""");
        DrawCodeCard(canvas, new RectF(left.X, left.Y + left.Height * .65f, left.Width, left.Height * .35f),
            "The drawing primitives", DrawingExample(_state.Champion));
        canvas.RestoreState();
    }

    private static string DrawingExample(Champion? champion) => champion switch
    {
        Champion.Charioteer => """
canvas.FillColor = Color.FromArgb("#D97706");
canvas.FillRectangle(
    x - 4.5f * p, y - 2.1f * p,
    5.8f * p, 2.1f * p);
""",
        Champion.SpacePilot => """
canvas.FillColor = Color.FromArgb("#6366F1");
canvas.FillRectangle(
    x - 4.5f * p, y - 2.2f * p,
    9f * p, 2.2f * p);
""",
        _ => """
canvas.FillColor = Color.FromArgb("#EF4444");
canvas.FillRectangle(
    x - 4.5f * p, y - 2.5f * p,
    9f * p, 2.8f * p);
"""
    };
}
