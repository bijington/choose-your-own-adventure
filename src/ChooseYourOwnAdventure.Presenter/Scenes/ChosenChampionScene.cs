using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

/// <summary>Meet the character, then evolve its rendering in four local steps.</summary>
public sealed class ChosenChampionScene : ChapterOpeningScene
{
    private readonly AdventureState _state;
    private Sprite? _sprite;

    public ChosenChampionScene(AdventureState state) : base(state,
        PresentationSlide.ChosenChampion, "THE AUDIENCE HAS CHOSEN", "OUR CHAMPION",
        string.Empty, showChampion: true) => _state = state;

    protected override string ContinuePrompt => "SOUTH / ENTER · DISCOVER OUR CHAMPION’S ORIGINS";

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        if (_state.ChampionRenderingStage == ChampionRenderingStage.Sprite)
        {
            EnsureSprite().Update(millisecondsSinceLastUpdate);
        }
        else
        {
            _sprite = null;
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        if (_state.ChampionRenderingStage is not { } stage)
        {
            base.Render(canvas, dimensions);
            return;
        }

        // Do not render the chapter's static character underneath the evolution.
        var step = ChampionRenderingWalkthrough.ForStage(stage);
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "OUR CHAMPION’S ORIGINS",
            $"{(int)stage + 1} / 4 · {step.Title}\nEvery champion has a backstory. Ours begins with a dot.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = CodeFonts.SemiBold;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(left.Width * .029f, 13f, 18f);
        canvas.DrawString("Render(ICanvas canvas, RectF dimensions)",
            left.X, left.Y, left.Width, left.Height * .10f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        DrawCodeCard(canvas, new RectF(left.X, left.Y + left.Height * .15f,
            left.Width, left.Height * .78f), step.Title, step.Code);

        var artwork = new RectF(right.X + right.Width * .15f, dimensions.Y + dimensions.Height * .30f,
            right.Width * .70f, dimensions.Height * .36f);
        DrawEvolution(canvas, artwork, stage);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 18f, 27f);
        canvas.DrawString(step.Caption, right.X, dimensions.Y + dimensions.Height * .69f,
            right.Width, dimensions.Height * .07f, HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawProgress(canvas, new RectF(right.X, dimensions.Y + dimensions.Height * .79f,
            right.Width, dimensions.Height * .055f), stage);
        DrawPrompt(canvas, dimensions, stage == ChampionRenderingStage.Sprite
            ? "SOUTH / RB / ENTER / PGDN · FOLLOW A FRAME OF THEIR LIFE"
            : "SOUTH / RB / ENTER / PGDN · CONTINUE THEIR ORIGIN STORY");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.ChosenChampion, _state.Champion));
    }

    private void DrawEvolution(ICanvas canvas, RectF bounds, ChampionRenderingStage stage)
    {
        var champion = _state.Champion ?? Champion.RacingDriver;
        canvas.SaveState();
        try
        {
            switch (stage)
            {
                case ChampionRenderingStage.Dot:
                    canvas.FillColor = Accent;
                    canvas.FillEllipse(bounds.Center.X - 14, bounds.Center.Y - 14, 28, 28);
                    break;
                case ChampionRenderingStage.Primitives:
                    DrawPrimitiveChampion(canvas, bounds, champion);
                    break;
                case ChampionRenderingStage.Image:
                    ChampionSprites.DrawCharacter(canvas, champion, bounds);
                    break;
                case ChampionRenderingStage.Sprite:
                    var sprite = EnsureSprite();
                    // The fixed frame canvas preserves the jump animation's anchor.
                    var height = Math.Min(bounds.Height, bounds.Width * 512f / 384f);
                    var width = height * 384f / 512f;
                    sprite.Bounds = new RectF(bounds.Center.X - width / 2f, bounds.Center.Y - height / 2f, width, height);
                    canvas.Antialias = false;
                    sprite.Render(canvas, bounds);
                    break;
            }
        }
        finally { canvas.RestoreState(); }
    }

    private static void DrawPrimitiveChampion(ICanvas canvas, RectF bounds, Champion champion)
    {
        var x = bounds.Center.X;
        var y = bounds.Center.Y;
        canvas.FillColor = champion switch
        {
            Champion.Charioteer => PresentationTheme.AccentGold,
            Champion.SpacePilot => PresentationTheme.CodePanelBorder,
            _ => Accent
        };
        canvas.FillEllipse(x - 16, y - 64, 32, 32);
        canvas.FillRectangle(x - 22, y - 25, 44, 54);
        canvas.StrokeColor = Ink;
        canvas.StrokeSize = 10;
        canvas.DrawLine(x - 12, y + 29, x - 20, y + 70);
        canvas.DrawLine(x + 12, y + 29, x + 20, y + 70);
        canvas.DrawLine(x - 22, y - 20, x - 42, y + 18);
        canvas.DrawLine(x + 22, y - 20, x + 42, y + 18);
    }

    private Sprite EnsureSprite() => _sprite ??= ChampionSprites.CreateCelebration(_state.Champion ?? Champion.RacingDriver);

    private static void DrawProgress(ICanvas canvas, RectF bounds, ChampionRenderingStage stage)
    {
        var gap = bounds.Width * .02f;
        var width = (bounds.Width - gap * 3) / 4;
        var stages = Enum.GetValues<ChampionRenderingStage>();
        for (var i = 0; i < stages.Length; i++)
        {
            var rect = new RectF(bounds.X + i * (width + gap), bounds.Y, width, bounds.Height);
            canvas.FillColor = stages[i] == stage ? PresentationTheme.CodePanelHeader : Panel;
            canvas.FillRoundedRectangle(rect, 8);
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = stages[i] == stage ? Accent : Muted;
            canvas.FontSize = Math.Clamp(width * .105f, 10f, 14f);
            var label = stages[i] == ChampionRenderingStage.Primitives ? "SHAPES" : stages[i].ToString().ToUpperInvariant();
            canvas.DrawString($"{i + 1} {label}", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
