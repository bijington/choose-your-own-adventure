using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class BranchResultScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public BranchResultScene(AdventureState state)
    {
        _state = state;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 16f, 26f);
        canvas.DrawString(
            "THE AUDIENCE HAS CHOSEN…",
            dimensions.X + 60,
            dimensions.Y + dimensions.Height * 0.17f,
            dimensions.Width - 120,
            42,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.052f, 40f, 76f);
        canvas.DrawString(
            GetChampionName(_state.Champion),
            dimensions.X + 70,
            dimensions.Y + dimensions.Height * 0.30f,
            dimensions.Width - 140,
            88,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        var branchPage = PresentationTheme.GetPageNumber(PresentationSlide.AddGameObject, _state.Champion);
        var card = new RectF(
            dimensions.X + dimensions.Width * 0.24f,
            dimensions.Y + dimensions.Height * 0.49f,
            dimensions.Width * 0.52f,
            dimensions.Height * 0.18f);

        canvas.FillColor = Color.FromArgb("#FFF0C2");
        canvas.FillRoundedRectangle(card, 22);
        canvas.StrokeColor = Gold;
        canvas.StrokeSize = 3;
        canvas.DrawRoundedRectangle(card, 22);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(card.Width * 0.038f, 17f, 26f);
        canvas.DrawString(
            "To continue your adventure…",
            card.X + 24,
            card.Y + 20,
            card.Width - 48,
            32,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(card.Width * 0.075f, 30f, 52f);
        canvas.DrawString(
            $"TURN TO PAGE {branchPage}",
            card.X + 20,
            card.Y + 62,
            card.Width - 40,
            100,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        DrawPrompt(canvas, dimensions, "A / ENTER  ·  TURN THE PAGE");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.BranchResult, _state.Champion));
    }

    private static string GetChampionName(Champion? champion) => champion switch
    {
        Champion.RacingDriver => "THE DRIVER",
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "OUR CHAMPION"
    };
}
