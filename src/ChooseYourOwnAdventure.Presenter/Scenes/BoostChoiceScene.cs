using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class BoostChoiceScene(AdventureState state, LiveRaceState race, QrCodeRenderer qr,
    LiveAudienceOptions options) : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        var snapshot = race.Snapshot;
        DrawTitle(canvas, dimensions, "YOU WRITE THE RULES",
            "You chose our champion. Now choose how EVERY team will boost in the final race.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 16, 26);
        canvas.DrawString("Steady effort… or a dramatic entrance?\n\nSame team balancing. Same total effort.\nTies use steady effort. Your team stays the same.",
            left.X, left.Y, left.Width, left.Height * .40f, HorizontalAlignment.Left, VerticalAlignment.Top);
        var size = Math.Min(left.Width * .36f, left.Height * .34f);
        qr.Draw(canvas, new RectF(left.X, left.Y + left.Height * .48f, size, size));
        canvas.FontSize = Math.Clamp(dimensions.Width * .011f, 11, 17);
        canvas.DrawString(options.JoinUrl, left.X, left.Y + left.Height * .85f, left.Width, 40,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        var closed = snapshot.BoostVotePhase == VotePhase.Closed;
        DrawPill(canvas, new RectF(right.X, right.Y, right.Width, right.Height * .30f),
            $"STEADY EFFORT · {snapshot.SteadyVotes} votes\nEvery tap moves your team.",
            closed ? snapshot.Mode == BoostMode.Steady : state.HighlightedBoostMode == BoostMode.Steady);
        DrawPill(canvas, new RectF(right.X, right.Y + right.Height * .38f, right.Width, right.Height * .30f),
            $"CHARGE & RELEASE · {snapshot.BurstVotes} votes\nFill the team meter. Automatically burst!",
            closed ? snapshot.Mode == BoostMode.ChargeAndRelease : state.HighlightedBoostMode == BoostMode.ChargeAndRelease);
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .017f, 16, 25);
        var status = closed ? $"CHOSEN: {(snapshot.Mode == BoostMode.Steady ? "STEADY EFFORT" : "CHARGE & RELEASE")}"
            : snapshot.BoostVotePhase == VotePhase.Open ? "VOTING OPEN · choose on your phone"
            : state.IsRehearsing ? "PRACTICE · tap an option to choose locally" : "SOUTH / ENTER · open the rule vote";
        canvas.DrawString(status, right.X, right.Y + right.Height * .78f, right.Width, right.Height * .18f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, closed ? "SOUTH / RB / ENTER / PGDN · CONNECT THE ROOM"
            : "SOUTH / ENTER · OPEN / CLOSE VOTE    LEFT PAGE · BACK");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.BoostChoice, state.Champion));
    }
}
