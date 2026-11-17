using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class PodiumScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    private readonly LiveRaceState _raceState;
    private readonly StaticChampionVehicleGameObject[] _vehicles = [new(), new(), new()];

    public PodiumScene(AdventureState state, LiveRaceState raceState)
    {
        _state = state;
        _raceState = raceState;
        foreach (var vehicle in _vehicles) Add(vehicle);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "THE PODIUM",
            "One final page in our adventure: the audience race results.");

        var standings = GetStandings();
        DrawPodium(canvas, dimensions, standings);
        base.Render(canvas, dimensions);
        DrawPrompt(canvas, dimensions, "SOUTH / ENTER  ·  RECAP THE ADVENTURE");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.Podium, _state.Champion));
    }

    private (Champion champion, double progress, int teamSize)[] GetStandings()
    {
        var snapshot = _raceState.Snapshot;
        (Champion champion, double progress, int teamSize)[] standings =
        [
            (Champion.RacingDriver, snapshot.RacingDriverProgress, snapshot.RacingDriverTeamSize),
            (Champion.Charioteer, snapshot.CharioteerProgress, snapshot.CharioteerTeamSize),
            (Champion.SpacePilot, snapshot.SpacePilotProgress, snapshot.SpacePilotTeamSize)
        ];

        return standings
        .OrderByDescending(x => x.progress)
        .ThenByDescending(x => x.teamSize)
        .ToArray();
    }

    private void DrawPodium(ICanvas canvas, RectF dimensions, (Champion champion, double progress, int teamSize)[] standings)
    {
        var baseY = dimensions.Y + dimensions.Height * 0.72f;
        var centerX = dimensions.X + dimensions.Width * 0.50f;
        var podiumWidth = dimensions.Width * 0.13f;
        var heights = new[] { dimensions.Height * 0.22f, dimensions.Height * 0.16f, dimensions.Height * 0.12f };
        var xs = new[] { centerX - podiumWidth / 2f, centerX - podiumWidth * 1.45f, centerX + podiumWidth * 0.45f };

        // second, first, third block layout visually

        for (var i = 0; i < standings.Length; i++)
        {
            var slot = i;
            var rect = new RectF(xs[slot], baseY - heights[i], podiumWidth, heights[i]);
            var champion = standings[i].champion;
            var fill = i == 0 ? Color.FromArgb("#F5D277") : i == 1 ? Color.FromArgb("#E6D3B0") : Color.FromArgb("#D7A56D");

            canvas.FillColor = fill;
            canvas.FillRoundedRectangle(rect, 14);
            canvas.StrokeColor = Border;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(rect, 14);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Ink;
            canvas.FontSize = Math.Clamp(rect.Width * 0.18f, 18f, 28f);
            canvas.DrawString((i + 1).ToString(), rect.X, rect.Y + 8, rect.Width, 48, HorizontalAlignment.Center, VerticalAlignment.Center);

            var cx = rect.X + rect.Width / 2f;
            var cy = rect.Y - 22f;
            _vehicles[i].Champion = champion;
            _vehicles[i].Position = new PointF(cx, cy);
            _vehicles[i].Scale = i == 0 ? 1.1f : 0.95f;

            canvas.FontColor = i == 0 ? Accent : Ink;
            canvas.FontSize = Math.Clamp(rect.Width * 0.13f, 12f, 19f);
            canvas.DrawString(GetChampionLabel(champion), rect.X - 40, rect.Y - 86, rect.Width + 80, 42, HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = Muted;
            canvas.FontSize = 12f;
            canvas.DrawString($"{Math.Round(standings[i].progress * 100):0}% · {standings[i].teamSize} racers", rect.X - 30, rect.Y - 52, rect.Width + 60, 18, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // Winner banner
        canvas.FillColor = Color.FromArgb("#FFF0C2");
        var banner = new RectF(dimensions.X + dimensions.Width * 0.33f, dimensions.Y + dimensions.Height * 0.22f, dimensions.Width * 0.34f, 56f);
        canvas.FillRoundedRectangle(banner, 18);
        canvas.StrokeColor = Accent;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(banner, 18);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(banner.Width * 0.06f, 18f, 28f);
        canvas.DrawString($"WINNER: {GetChampionLabel(standings[0].champion)}", banner.X + 12, banner.Y + 10, banner.Width - 24, 36, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static string GetChampionLabel(Champion champion) => champion switch
    {
        Champion.RacingDriver => "THE DRIVER",
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "THE CHAMPION"
    };

}
