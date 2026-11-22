using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OpeningScene : GameScene
{
    private readonly AdventureState _state;
    private readonly AudienceVoteClient _audience;
    private readonly LiveVoteState _liveVoteState;
    private readonly LiveAudienceOptions _liveAudienceOptions;
    private readonly QrCodeRenderer _qrCodeRenderer;
    private double _elapsedMilliseconds;
    private bool _votePhaseInitialized;
    private VotePhase _displayedVotePhase;
    private double _votePhaseElapsedMilliseconds;
    private float _displayedDriverVotes;
    private float _displayedCharioteerVotes;
    private float _displayedSpacePilotVotes;
    private Champion? _celebratingChampion;
    private Sprite? _winnerCelebration;

    private const double LockInDurationMilliseconds = 3000d;
    private const double RevealSuspenseMilliseconds = 1100d;
    private const double RevealFadeMilliseconds = 900d;

    public OpeningScene(
        AdventureState state,
        AudienceVoteClient audience,
        LiveVoteState liveVoteState,
        LiveAudienceOptions liveAudienceOptions,
        QrCodeRenderer qrCodeRenderer)
    {
        _state = state;
        _audience = audience;
        _liveVoteState = liveVoteState;
        _liveAudienceOptions = liveAudienceOptions;
        _qrCodeRenderer = qrCodeRenderer;
    }

    private VoteSnapshot CurrentVotes => _state.LocalVoteWinner is { } local
        ? new VoteSnapshot(_liveAudienceOptions.SessionId, 0, 0, 0, VotePhase.Revealed, local)
        : _audience.IsConnected ? _liveVoteState.Snapshot
        : new VoteSnapshot(_liveAudienceOptions.SessionId, 0, 0, 0, VotePhase.Ready, null);

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _elapsedMilliseconds += millisecondsSinceLastUpdate;
        var votes = CurrentVotes;
        if (!_votePhaseInitialized || votes.Phase != _displayedVotePhase)
        {
            _displayedVotePhase = votes.Phase;
            _votePhaseElapsedMilliseconds = 0d;
            _votePhaseInitialized = true;
        }
        else
        {
            _votePhaseElapsedMilliseconds += millisecondsSinceLastUpdate;
        }
        var smoothing = (float)(1d - Math.Exp(-millisecondsSinceLastUpdate / 180d));
        _displayedDriverVotes += (votes.RacingDriver - _displayedDriverVotes) * smoothing;
        _displayedCharioteerVotes += (votes.Charioteer - _displayedCharioteerVotes) * smoothing;
        _displayedSpacePilotVotes += (votes.SpacePilot - _displayedSpacePilotVotes) * smoothing;

        // Start on frame zero when the winner becomes visible, after the suspense.
        // Clearing this on reset/tie also makes a subsequent reveal start fresh.
        var winner = votes.Phase == VotePhase.Revealed &&
            _votePhaseElapsedMilliseconds > RevealSuspenseMilliseconds
            ? votes.RevealedChampion : null;
        if (winner != _celebratingChampion)
        {
            _winnerCelebration?.Stop();
            _celebratingChampion = winner;
            _winnerCelebration = winner is { } champion
                ? ChampionSprites.CreateCelebration(champion) : null;
        }
        else
        {
            _winnerCelebration?.Update(millisecondsSinceLastUpdate);
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        // Voting stays visually stable when accepting the winner changes AdventureState.
        // BookScene captures this page before loading the chapter reveal.
        PresentationTheme.DrawParchment(canvas, dimensions);
        DrawChampionSelection(canvas, dimensions, _state.HighlightedChampion);
    }

    private void DrawChampionSelection(ICanvas canvas, RectF dimensions, Champion highlightedChampion)
    {
        var votes = CurrentVotes;
        var revealProgress = votes.Phase == VotePhase.Revealed
            ? Math.Clamp((_votePhaseElapsedMilliseconds - RevealSuspenseMilliseconds) / RevealFadeMilliseconds, 0d, 1d)
            : 0d;
        var revealWinner = revealProgress > 0d ? votes.RevealedChampion : null;
        var hasReveal = votes.Phase == VotePhase.Revealed && revealProgress > 0d;
        var displayedTotal = Math.Max(0.001f, _displayedDriverVotes + _displayedCharioteerVotes + _displayedSpacePilotVotes);

        var left = BookLayout.LeftPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .025f, 24f, 38f);
        canvas.DrawString("CHOOSE YOUR CHAMPION", left.X, dimensions.Y + dimensions.Height * .055f,
            left.Width, dimensions.Height * .12f, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .017f, 16f, 25f);
        canvas.DrawString(_audience.IsConnected && _state.LocalVoteWinner is null
            ? "Scan the code and choose a champion on your phone.\n\nThe winning choice shapes our adventure.\nYour vote also chooses your team for the audience race."
            : "Choose a champion for our adventure.\n\nMove the left stick up or down to highlight a champion, then press South to confirm.\nYou can also use the arrow keys or tap a champion card.",
            left.X, dimensions.Y + dimensions.Height * .22f, left.Width, dimensions.Height * .25f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 13f, 21f);
        canvas.DrawString(GetVotingSubtitle(votes), left.X, dimensions.Y + dimensions.Height * .465f,
            left.Width, dimensions.Height * .075f, HorizontalAlignment.Left, VerticalAlignment.Center);

        var choices = new[]
        {
            (Champion.RacingDriver, "THE DRIVER", "Speed. Precision.", "Questionable braking.", votes.RacingDriver, _displayedDriverVotes),
            (Champion.Charioteer, "THE CHARIOTEER", "Glory. Courage.", "Undefined behaviour.", votes.Charioteer, _displayedCharioteerVotes),
            (Champion.SpacePilot, "THE SPACE PILOT", "Exploration. Thrusters.", "Naturally, Orbit.", votes.SpacePilot, _displayedSpacePilotVotes)
        };
        // Draw the growing winner last so the other cards fade beneath it.
        foreach (var choice in choices.OrderBy(choice => choice.Item1 == revealWinner))
        {
            var rect = ChampionSelectionLayout.Card(dimensions, choice.Item1);
            if (choice.Item1 == revealWinner)
            {
                var target = BookLayout.RightPage(dimensions);
                var t = EaseInOut((float)revealProgress);
                rect = new RectF(rect.X + (target.X - rect.X) * t,
                    rect.Y + (target.Y - rect.Y) * t,
                    rect.Width + (target.Width - rect.Width) * t,
                    rect.Height + (target.Height - rect.Height) * t);
            }
            DrawChampionCard(canvas, rect, choice.Item2, choice.Item3, choice.Item4, choice.Item1,
                choice.Item5, choice.Item6 / displayedTotal, highlightedChampion == choice.Item1,
                revealWinner == choice.Item1, hasReveal, (float)revealProgress);
        }

        var qrSize = Math.Clamp(dimensions.Height * .20f, 110f, 210f);
        var qrRect = new RectF(left.X, dimensions.Y + dimensions.Height * .57f, qrSize, qrSize);
        _qrCodeRenderer.Draw(canvas, qrRect);
        var infoX = qrRect.Right + 20;
        var infoWidth = left.Right - infoX;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 14f, 22f);
        canvas.DrawString(GetVotingStatus(votes), infoX, qrRect.Y, infoWidth, 64,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * .012f, 12f, 18f);
        canvas.DrawString($"LIVE VOTES: {votes.Total}", infoX, qrRect.Y + 76, infoWidth, 32,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.DrawString(_liveAudienceOptions.JoinUrl, left.X, qrRect.Bottom + 12, left.Width, 44,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.DrawString(GetPresenterPrompt(votes, _votePhaseElapsedMilliseconds), left.X,
            dimensions.Y + dimensions.Height * .85f, left.Width, dimensions.Height * .09f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        if (votes.Phase == VotePhase.Closed && _votePhaseElapsedMilliseconds < LockInDurationMilliseconds)
        {
            DrawLockInCountdown(canvas, dimensions);
        }

        if (votes.Phase == VotePhase.Revealed && _votePhaseElapsedMilliseconds < RevealSuspenseMilliseconds)
        {
            DrawRevealSuspense(canvas, dimensions);
        }
        else if (votes.Phase == VotePhase.Revealed && votes.RevealedChampion is null)
        {
            DrawTieBanner(canvas, dimensions);
        }

        PresentationTheme.DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.ChooseChampion));
    }

    private void DrawChampionCard(ICanvas canvas, RectF rect, string title, string line1, string line2,
        Champion champion, int voteCount, float voteShare, bool isHighlighted, bool isWinner, bool hasReveal, float revealProgress)
    {
        if (hasReveal && !isWinner)
        {
            canvas.Alpha = 1f - revealProgress;
        }

        canvas.FillColor = isWinner
            ? Color.FromArgb("#FFF0C2")
            : isHighlighted ? Color.FromArgb("#FFF4D8") : PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(rect, 18);

        var winnerPulse = (Math.Sin(_elapsedMilliseconds / 180d) + 1d) / 2d;
        canvas.StrokeColor = isWinner
            ? PresentationTheme.AccentGold
            : isHighlighted ? PresentationTheme.AccentRed : PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = isWinner ? (float)(4d + winnerPulse * 4d) : isHighlighted ? 4 : 2;
        canvas.DrawRoundedRectangle(rect, 18);

        var spriteWidth = rect.Width * .18f;
        var artwork = new RectF(rect.X + 16f, rect.Y + rect.Height * .06f, spriteWidth, rect.Height * .55f);
        var textX = rect.X + spriteWidth + 28f;
        var textWidth = rect.Right - textX - 16f;
        var titleHeight = rect.Height * .29f;
        var descriptionY = rect.Y + rect.Height * .36f;
        var descriptionHeight = rect.Height * .25f;
        if (isWinner)
        {
            var t = EaseInOut(revealProgress);
            var target = new RectF(rect.X + rect.Width * .20f, rect.Y + rect.Height * .30f,
                rect.Width * .60f, rect.Height * .34f);
            artwork = new RectF(artwork.X + (target.X - artwork.X) * t,
                artwork.Y + (target.Y - artwork.Y) * t,
                artwork.Width + (target.Width - artwork.Width) * t,
                artwork.Height + (target.Height - artwork.Height) * t);
            textX += (rect.X + 20f - textX) * t;
            textWidth = rect.Right - textX - 20f;
            // Shrink the heading's layout box as well as moving the description:
            // vertically centering text in the old tall box made the two overlap.
            titleHeight -= rect.Height * .17f * t;
            descriptionY -= rect.Height * .16f * t;
            descriptionHeight -= rect.Height * .175f * t;
        }

        if (isWinner && _celebratingChampion == champion && _winnerCelebration is { } celebration)
        {
            // Orbit renders into Sprite.Bounds rather than the Render dimensions.
            // All celebration frames are 384 x 512; keep that fixed canvas ratio.
            var height = Math.Min(artwork.Height, artwork.Width * 512f / 384f);
            var width = height * 384f / 512f;
            celebration.Bounds = new RectF(artwork.X + (artwork.Width - width) / 2f,
                artwork.Y + (artwork.Height - height) / 2f, width, height);
            canvas.SaveState();
            try
            {
                canvas.Antialias = false;
                celebration.Render(canvas, rect);
            }
            finally { canvas.RestoreState(); }
        }
        else
        {
            ChampionSprites.DrawCharacter(canvas, champion, artwork);
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = isWinner ? PresentationTheme.AccentRed : PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(Math.Min(rect.Width * (isWinner ? .065f : .044f), rect.Height * .13f), 16f, isWinner ? 38f : 26f);
        canvas.DrawString(isWinner ? $"★ {title}" : title, textX, rect.Y + rect.Height * .06f,
            textWidth, titleHeight, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(Math.Min(rect.Width * .031f, rect.Height * .10f), 12f, 18f);
        canvas.DrawString($"{line1} {line2}", textX, descriptionY,
            textWidth, descriptionHeight, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = voteCount > 0 ? PresentationTheme.AccentRed : PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(rect.Height * .10f, 12f, 19f);
        canvas.DrawString($"{voteCount} VOTE{(voteCount == 1 ? string.Empty : "S")}",
            rect.X + 20, rect.Y + rect.Height * .65f, rect.Width * .32f, rect.Height * .25f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        var destination = PresentationTheme.GetPageNumber(PresentationSlide.ChosenChampion, champion);
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.DrawString($"TURN TO PAGE {destination}", rect.X + rect.Width * .35f,
            rect.Y + rect.Height * .65f, rect.Width * .65f - 20, rect.Height * .25f,
            HorizontalAlignment.Right, VerticalAlignment.Center);

        var barRect = new RectF(rect.X + 20, rect.Bottom - 12, rect.Width - 40, 4);
        canvas.FillColor = Color.FromArgb("#E3CFA4");
        canvas.FillRoundedRectangle(barRect, 2);
        if (voteShare > 0f)
        {
            canvas.FillColor = PresentationTheme.AccentRed;
            canvas.FillRoundedRectangle(new RectF(barRect.X, barRect.Y,
                Math.Clamp(barRect.Width * voteShare, 2f, barRect.Width), barRect.Height), 2);
        }

        canvas.Alpha = 1f;
    }

    private string GetVotingSubtitle(VoteSnapshot votes) => _state.LocalVoteWinner is not null
        ? "Our champion has been chosen locally."
        : !_audience.IsConnected ? _state.IsRehearsing ? "PRACTICE MODE — choose a champion locally." : "AUDIENCE OFFLINE — choose a champion locally, or wait for reconnection."
        : votes.Phase switch
    {
        VotePhase.Ready => "Scan the QR code now. Voting opens when the presenter starts the round.",
        VotePhase.Open => "Voting is OPEN — choose carefully. This decision themes the rest of the session.",
        VotePhase.Closed => "Votes are locked. Hold that thought…",
        VotePhase.Revealed when votes.RevealedChampion is null => "It is a tie! The room must decide again.",
        VotePhase.Revealed => "The audience has chosen.",
        _ => string.Empty
    };

    private string GetVotingStatus(VoteSnapshot votes) => _state.LocalVoteWinner is not null
        ? "LOCAL CHAMPION"
        : !_audience.IsConnected ? _state.IsRehearsing ? "PRACTICE · LOCAL SELECTION" : "OFFLINE · LOCAL SELECTION"
        : votes.Phase switch
    {
        VotePhase.Ready => "SCAN TO JOIN",
        VotePhase.Open => "● VOTING OPEN",
        VotePhase.Closed => "LOCKING IN…",
        VotePhase.Revealed when votes.RevealedChampion is null => "TIE!",
        VotePhase.Revealed => "WINNER REVEALED",
        _ => "SCAN TO VOTE"
    };

    private string GetPresenterPrompt(VoteSnapshot votes, double phaseElapsedMilliseconds) =>
        !_audience.IsConnected && _state.LocalVoteWinner is null
        ? "STICK UP / DOWN · SELECT   •   SOUTH / ENTER · CONFIRM   •   LB / PGUP · BACK"
        : votes.Phase switch
    {
        VotePhase.Ready => "SOUTH / ENTER  ·  OPEN VOTING   •   EAST / SHIFT  ·  RESET   •   LB / PGUP / LEFT PAGE  ·  BACK",
        VotePhase.Open => "SOUTH / ENTER  ·  CLOSE VOTING   •   EAST / SHIFT  ·  RESET   •   LB / PGUP / LEFT PAGE  ·  BACK",
        VotePhase.Closed when phaseElapsedMilliseconds < LockInDurationMilliseconds => "LOCKING VOTES…",
        VotePhase.Closed => "SOUTH / ENTER  ·  REVEAL WINNER   •   EAST / SHIFT  ·  RESET   •   LB / PGUP / LEFT PAGE  ·  BACK",
        VotePhase.Revealed when phaseElapsedMilliseconds < RevealSuspenseMilliseconds => "AND THE AUDIENCE CHOSE…",
        VotePhase.Revealed when votes.RevealedChampion is null => "SOUTH / ENTER  ·  REOPEN VOTING   •   EAST / SHIFT  ·  RESET   •   LB / PGUP / LEFT PAGE  ·  BACK",
        VotePhase.Revealed => "SOUTH / ENTER  ·  CONTINUE WITH THE WINNER   •   EAST / SHIFT  ·  RESET   •   LB / PGUP / LEFT PAGE  ·  BACK",
        _ => string.Empty
    };

    private void DrawLockInCountdown(ICanvas canvas, RectF dimensions)
    {
        var secondsRemaining = Math.Max(1, 3 - (int)(_votePhaseElapsedMilliseconds / 1000d));
        var pulse = (float)((Math.Sin(_elapsedMilliseconds / 120d) + 1d) / 2d);
        var left = BookLayout.LeftPage(dimensions);
        var box = new RectF(left.X, dimensions.Y + dimensions.Height * .25f,
            left.Width, dimensions.Height * .25f);

        canvas.FillColor = Color.FromRgba(255, 240, 194, 242);
        canvas.FillRoundedRectangle(box, 24);
        canvas.StrokeColor = PresentationTheme.AccentRed;
        canvas.StrokeSize = 3f + pulse * 3f;
        canvas.DrawRoundedRectangle(box, 24);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.CodeMetric;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.075f, 54f, 104f);
        canvas.DrawString(secondsRemaining.ToString(), box, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 21f);
        canvas.DrawString("LOCKING IN", box.X, box.Bottom - 48, box.Width, 28,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawRevealSuspense(ICanvas canvas, RectF dimensions)
    {
        var progress = Math.Clamp(_votePhaseElapsedMilliseconds / RevealSuspenseMilliseconds, 0d, 1d);
        canvas.Alpha = (float)(0.72d + progress * 0.20d);
        canvas.FillColor = Color.FromArgb("#FFF0C2");
        var left = BookLayout.LeftPage(dimensions);
        var banner = new RectF(left.X, dimensions.Y + dimensions.Height * .25f,
            left.Width, dimensions.Height * .20f);
        canvas.FillRoundedRectangle(banner, 24);
        canvas.StrokeColor = PresentationTheme.AccentRed;
        canvas.StrokeSize = 3;
        canvas.DrawRoundedRectangle(banner, 24);
        canvas.Alpha = 1f;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.020f, 20f, 30f);
        canvas.DrawString("AND THE AUDIENCE CHOSE…", banner, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawTieBanner(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = PresentationTheme.AccentRed;
        var left = BookLayout.LeftPage(dimensions);
        var banner = new RectF(left.X, dimensions.Y + dimensions.Height * .19f, left.Width, 42);
        canvas.FillRoundedRectangle(banner, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 20f);
        canvas.DrawString("TIE — ONE MORE ROUND!", banner, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static float EaseInOut(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return value * value * (3f - 2f * value);
    }
}
