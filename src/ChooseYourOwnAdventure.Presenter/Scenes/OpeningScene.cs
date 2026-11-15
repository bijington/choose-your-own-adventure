using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OpeningScene : GameScene
{
    private readonly AdventureState _state;
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

    private const double LockInDurationMilliseconds = 3000d;
    private const double RevealSuspenseMilliseconds = 1100d;
    private const double RevealFadeMilliseconds = 900d;
    private const double CodeRevealTransitionMilliseconds = 1400d;

    public OpeningScene(
        AdventureState state,
        LiveVoteState liveVoteState,
        LiveAudienceOptions liveAudienceOptions,
        QrCodeRenderer qrCodeRenderer)
    {
        _state = state;
        _liveVoteState = liveVoteState;
        _liveAudienceOptions = liveAudienceOptions;
        _qrCodeRenderer = qrCodeRenderer;
        Add(new ChampionGameObject(state));
    }

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _elapsedMilliseconds += millisecondsSinceLastUpdate;

        var votes = _liveVoteState.Snapshot;
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

        if (_state.OpeningStage == OpeningStage.ChampionTransition)
        {
            var progress = _state.CodeRevealTransitionProgress
                + (float)(millisecondsSinceLastUpdate / CodeRevealTransitionMilliseconds);
            _state.SetCodeRevealTransitionProgress(progress);

            if (progress >= 1f)
            {
                _state.CompleteCodeRevealTransition();
            }
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);

        if (_state.OpeningStage == OpeningStage.ChampionTransition)
        {
            DrawCodeRevealTransition(canvas, dimensions, _state.CodeRevealTransitionProgress);
            base.Render(canvas, dimensions);
            return;
        }

        if (_state.OpeningStage is OpeningStage.CodeReveal or OpeningStage.Complete)
        {
            DrawCodeRevealShell(canvas, dimensions);
            base.Render(canvas, dimensions);
            DrawCodePanel(canvas, GetCodePanel(dimensions));
            DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  CONTINUE TO THE GAME LOOP", 0.88f);
            return;
        }

        base.Render(canvas, dimensions);

        switch (_state.OpeningStage)
        {
            case OpeningStage.Title:
                DrawTitle(canvas, dimensions);
                break;
            case OpeningStage.Awakening:
                DrawDialogue(canvas, dimensions, "You awaken in a strange conference hall...");
                break;
            case OpeningStage.Memory:
                DrawDialogue(canvas, dimensions, "You remember nothing.");
                break;
            case OpeningStage.DotNetMemory:
                DrawDialogue(canvas, dimensions, "Except C#.");
                break;
            case OpeningStage.DependencyInjection:
                DrawDialogue(canvas, dimensions, "And, strangely...", "dependency injection.");
                break;
            case OpeningStage.ChooseChampion:
                DrawChampionSelection(canvas, dimensions, _state.HighlightedChampion);
                break;
            case OpeningStage.ChampionChosen:
                DrawChampionChosen(canvas, dimensions);
                break;
        }
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        PresentationTheme.DrawParchment(canvas, dimensions);
    }

    private void DrawTitle(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.045f, 28f, 64f);
        canvas.DrawString("CHOOSE YOUR OWN ADVENTURE", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.28f, Math.Max(0, dimensions.Width - 80), 90,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 15f, 26f);
        canvas.DrawString("Built with .NET MAUI + Orbit", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.45f, Math.Max(0, dimensions.Width - 80), 50,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "PRESS START", 0.64f);
    }

    private void DrawDialogue(ICanvas canvas, RectF dimensions, string line1, string? line2 = null)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.033f, 24f, 48f);
        canvas.DrawString(line1, dimensions.X + dimensions.Width * 0.10f,
            dimensions.Y + dimensions.Height * 0.37f, dimensions.Width * 0.80f, 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        if (!string.IsNullOrWhiteSpace(line2))
        {
            canvas.FontColor = PresentationTheme.AccentRed;
            canvas.DrawString(line2, dimensions.X + dimensions.Width * 0.10f,
                dimensions.Y + dimensions.Height * 0.49f, dimensions.Width * 0.80f, 70,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  CONTINUE", 0.72f);
    }

    private void DrawContinuePrompt(ICanvas canvas, RectF dimensions, string text, float yFactor)
    {
        var pulse = (Math.Sin(_elapsedMilliseconds / 450d) + 1d) / 2d;
        canvas.Alpha = (float)(0.35d + pulse * 0.65d);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 16f, 28f);
        canvas.DrawString(text, dimensions.X + 40, dimensions.Y + dimensions.Height * yFactor,
            Math.Max(0, dimensions.Width - 80), 50, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Alpha = 1f;
    }

    private void DrawChampionSelection(ICanvas canvas, RectF dimensions, Champion highlightedChampion)
    {
        var votes = _liveVoteState.Snapshot;
        var revealProgress = votes.Phase == VotePhase.Revealed
            ? Math.Clamp((_votePhaseElapsedMilliseconds - RevealSuspenseMilliseconds) / RevealFadeMilliseconds, 0d, 1d)
            : 0d;
        var revealWinner = revealProgress > 0d ? votes.RevealedChampion : null;
        var hasReveal = votes.Phase == VotePhase.Revealed && revealProgress > 0d;
        var displayedTotal = Math.Max(0.001f, _displayedDriverVotes + _displayedCharioteerVotes + _displayedSpacePilotVotes);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.034f, 26f, 50f);
        canvas.DrawString("CHOOSE YOUR CHAMPION", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.08f, Math.Max(0, dimensions.Width - 80), 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString(GetVotingSubtitle(votes), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.17f, Math.Max(0, dimensions.Width - 80), 40,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        var cardTop = dimensions.Y + dimensions.Height * 0.27f;
        var cardHeight = dimensions.Height * 0.34f;
        var gap = Math.Max(18f, dimensions.Width * 0.018f);
        var totalWidth = dimensions.Width * 0.86f;
        var cardWidth = (totalWidth - gap * 2) / 3f;
        var startX = dimensions.X + (dimensions.Width - totalWidth) / 2f;

        DrawChampionCard(canvas, new RectF(startX, cardTop, cardWidth, cardHeight),
            "THE DRIVER", "Speed. Precision.", "Questionable braking.", "01", votes.RacingDriver, _displayedDriverVotes / displayedTotal,
            highlightedChampion == Champion.RacingDriver, revealWinner == Champion.RacingDriver, hasReveal, (float)revealProgress);
        DrawChampionCard(canvas, new RectF(startX + cardWidth + gap, cardTop, cardWidth, cardHeight),
            "THE CHARIOTEER", "Glory. Courage.", "Undefined behaviour.", "02", votes.Charioteer, _displayedCharioteerVotes / displayedTotal,
            highlightedChampion == Champion.Charioteer, revealWinner == Champion.Charioteer, hasReveal, (float)revealProgress);
        DrawChampionCard(canvas, new RectF(startX + (cardWidth + gap) * 2, cardTop, cardWidth, cardHeight),
            "THE SPACE PILOT", "Exploration. Thrusters.", "Naturally, Orbit.", "03", votes.SpacePilot, _displayedSpacePilotVotes / displayedTotal,
            highlightedChampion == Champion.SpacePilot, revealWinner == Champion.SpacePilot, hasReveal, (float)revealProgress);

        var qrSize = Math.Clamp(dimensions.Height * 0.19f, 105f, 170f);
        var qrRect = new RectF(dimensions.X + dimensions.Width * 0.12f,
            dimensions.Y + dimensions.Height * 0.66f, qrSize, qrSize);
        _qrCodeRenderer.Draw(canvas, qrRect);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = votes.Phase == VotePhase.Open ? PresentationTheme.AccentRed : PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.016f, 14f, 23f);
        canvas.DrawString(GetVotingStatus(votes), qrRect.Right + 28, qrRect.Y + 8, dimensions.Width * 0.50f, 32,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 11f, 18f);
        canvas.DrawString(_liveAudienceOptions.JoinUrl, qrRect.Right + 28, qrRect.Y + 42,
            dimensions.Width * 0.55f, 28, HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.DrawString($"LIVE VOTES: {votes.Total}", qrRect.Right + 28, qrRect.Y + 75,
            dimensions.Width * 0.30f, 28, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.FontColor = PresentationTheme.CodeMuted;
        canvas.DrawString(GetPresenterPrompt(votes, _votePhaseElapsedMilliseconds),
            qrRect.Right + 28, qrRect.Y + 108, dimensions.Width * 0.58f, 52,
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
        string number, int voteCount, float voteShare, bool isHighlighted, bool isWinner, bool hasReveal, float revealProgress)
    {
        if (hasReveal && !isWinner)
        {
            canvas.Alpha = 1f - (0.65f * revealProgress);
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

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.CodeMetric;
        canvas.FontSize = Math.Clamp(rect.Width * 0.13f, 24f, 42f);
        canvas.DrawString(number, rect.X, rect.Y + rect.Height * 0.07f, rect.Width, rect.Height * 0.22f,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(rect.Width * 0.070f, 14f, 23f);
        canvas.DrawString(title, rect.X + 10, rect.Y + rect.Height * 0.37f, rect.Width - 20, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(rect.Width * 0.046f, 12f, 18f);
        canvas.DrawString(line1, rect.X + 10, rect.Y + rect.Height * 0.53f, rect.Width - 20, 28,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.DrawString(line2, rect.X + 10, rect.Y + rect.Height * 0.63f, rect.Width - 20, 28,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        var barRect = new RectF(rect.X + rect.Width * 0.10f, rect.Y + rect.Height * 0.73f, rect.Width * 0.80f, 8f);
        canvas.FillColor = Color.FromArgb("#E3CFA4");
        canvas.FillRoundedRectangle(barRect, 4);
        if (voteShare > 0f)
        {
            var animatedWidth = Math.Clamp(barRect.Width * voteShare, 2f, barRect.Width);
            canvas.FillColor = PresentationTheme.AccentRed;
            canvas.FillRoundedRectangle(new RectF(barRect.X, barRect.Y, animatedWidth, barRect.Height), 4);
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = voteCount > 0 ? PresentationTheme.AccentRed : PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(rect.Width * 0.055f, 12f, 19f);
        canvas.DrawString($"{voteCount} VOTE{(voteCount == 1 ? string.Empty : "S")}", rect.X + 10,
            rect.Y + rect.Height * 0.80f, rect.Width - 20, 30, HorizontalAlignment.Center, VerticalAlignment.Center);

        if (isWinner)
        {
            canvas.FontColor = PresentationTheme.CodeMetric;
            canvas.FontSize = Math.Clamp(rect.Width * 0.045f, 11f, 17f);
            canvas.DrawString("★ AUDIENCE CHAMPION ★", rect.X + 10,
                rect.Bottom - 28, rect.Width - 20, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        canvas.Alpha = 1f;
    }

    private static string GetVotingSubtitle(VoteSnapshot votes) => votes.Phase switch
    {
        VotePhase.Ready => "Scan the QR code now. Voting opens when the presenter starts the round.",
        VotePhase.Open => "Voting is OPEN — choose carefully. This decision themes the rest of the session.",
        VotePhase.Closed => "Votes are locked. Hold that thought…",
        VotePhase.Revealed when votes.RevealedChampion is null => "It is a tie! The room must decide again.",
        VotePhase.Revealed => "The audience has chosen.",
        _ => string.Empty
    };

    private static string GetVotingStatus(VoteSnapshot votes) => votes.Phase switch
    {
        VotePhase.Ready => "SCAN TO JOIN",
        VotePhase.Open => "● VOTING OPEN",
        VotePhase.Closed => "LOCKING IN…",
        VotePhase.Revealed when votes.RevealedChampion is null => "TIE!",
        VotePhase.Revealed => "WINNER REVEALED",
        _ => "SCAN TO VOTE"
    };

    private static string GetPresenterPrompt(VoteSnapshot votes, double phaseElapsedMilliseconds) => votes.Phase switch
    {
        VotePhase.Ready => "A / ENTER  ·  OPEN VOTING   •   B / ESC  ·  RESET",
        VotePhase.Open => "A / ENTER  ·  CLOSE VOTING   •   B / ESC  ·  RESET",
        VotePhase.Closed when phaseElapsedMilliseconds < LockInDurationMilliseconds => "LOCKING VOTES…",
        VotePhase.Closed => "A / ENTER  ·  REVEAL WINNER   •   B / ESC  ·  RESET",
        VotePhase.Revealed when phaseElapsedMilliseconds < RevealSuspenseMilliseconds => "AND THE AUDIENCE CHOSE…",
        VotePhase.Revealed when votes.RevealedChampion is null => "A / ENTER  ·  REOPEN VOTING   •   B / ESC  ·  RESET",
        VotePhase.Revealed => "A / ENTER  ·  CONTINUE WITH THE WINNER   •   B / ESC  ·  RESET",
        _ => string.Empty
    };

    private void DrawLockInCountdown(ICanvas canvas, RectF dimensions)
    {
        var secondsRemaining = Math.Max(1, 3 - (int)(_votePhaseElapsedMilliseconds / 1000d));
        var pulse = (float)((Math.Sin(_elapsedMilliseconds / 120d) + 1d) / 2d);
        var box = new RectF(dimensions.X + dimensions.Width * 0.39f,
            dimensions.Y + dimensions.Height * 0.30f, dimensions.Width * 0.22f, dimensions.Height * 0.25f);

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
        var banner = new RectF(dimensions.X + dimensions.Width * 0.18f,
            dimensions.Y + dimensions.Height * 0.31f, dimensions.Width * 0.64f, dimensions.Height * 0.18f);
        canvas.FillRoundedRectangle(banner, 24);
        canvas.StrokeColor = PresentationTheme.AccentRed;
        canvas.StrokeSize = 3;
        canvas.DrawRoundedRectangle(banner, 24);
        canvas.Alpha = 1f;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.028f, 24f, 42f);
        canvas.DrawString("AND THE AUDIENCE CHOSE…", banner, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawTieBanner(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = PresentationTheme.AccentRed;
        var banner = new RectF(dimensions.X + dimensions.Width * 0.34f,
            dimensions.Y + dimensions.Height * 0.21f, dimensions.Width * 0.32f, 42);
        canvas.FillRoundedRectangle(banner, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 20f);
        canvas.DrawString("TIE — ONE MORE ROUND!", banner, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawChampionChosen(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.04f, 28f, 56f);
        canvas.DrawString(GetChampionName(_state.Champion), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.09f, Math.Max(0, dimensions.Width - 80), 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.019f, 16f, 28f);
        canvas.DrawString(GetThemeTagline(_state.Champion), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.18f, Math.Max(0, dimensions.Width - 80), 50,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  REVEAL THE ORBIT CODE", 0.88f);
    }

    private void DrawCodeRevealTransition(ICanvas canvas, RectF dimensions, float progress)
    {
        var eased = EaseInOut(progress);

        canvas.Alpha = Math.Clamp((eased - 0.18f) / 0.52f, 0f, 1f);
        DrawCodeRevealShell(canvas, dimensions);

        var codePanel = GetCodePanel(dimensions);
        var slide = (1f - eased) * Math.Min(90f, dimensions.Width * 0.08f);
        codePanel = new RectF(codePanel.X - slide, codePanel.Y, codePanel.Width, codePanel.Height);
        DrawCodePanel(canvas, codePanel);
        canvas.Alpha = 1f;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 21f);
        canvas.Alpha = Math.Clamp(1f - eased * 1.6f, 0f, 1f);
        canvas.DrawString("THE CHAMPION BECOMES A GAMEOBJECT…", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.87f, Math.Max(0, dimensions.Width - 80), 36,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Alpha = 1f;
    }

    private static float EaseInOut(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return value * value * (3f - 2f * value);
    }

    private static RectF GetCodePanel(RectF dimensions)
    {
        var margin = Math.Max(42f, dimensions.Width * 0.05f);
        var gap = Math.Max(28f, dimensions.Width * 0.035f);
        var contentTop = dimensions.Y + dimensions.Height * 0.16f;
        var contentHeight = dimensions.Height * 0.66f;
        var panelWidth = (dimensions.Width - margin * 2 - gap) / 2f;
        return new RectF(dimensions.X + margin, contentTop, panelWidth, contentHeight);
    }

    private static void DrawCodeRevealShell(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.032f, 28f, 48f);
        canvas.DrawString("REAL ORBIT CODE → LIVE GAMEOBJECT", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.055f, Math.Max(0, dimensions.Width - 80), 60,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        var left = GetCodePanel(dimensions);
        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(left, 18);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(left, 18);
    }

    private void DrawCodePanel(ICanvas canvas, RectF rect)
    {
        canvas.Font = CodeFonts.SemiBold;
        canvas.FontColor = PresentationTheme.CodeMuted;
        canvas.FontSize = Math.Clamp(rect.Width * 0.048f, 16f, 24f);
        canvas.DrawString("OpeningScene.cs", rect.X + 24, rect.Y + 14, rect.Width - 48, 26,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        string[] lines =
        [
            "// OpeningScene",
            "Add(new ChampionGameObject(state));",
            "",
            "public sealed class ChampionGameObject",
            "    : GameObject",
            "{",
            "    public override void Render(",
            "        ICanvas canvas, RectF dimensions)",
            "    {",
            "        DrawChampion(canvas, dimensions,",
            "            _state.Champion);",
            "    }",
            "}"
        ];

        canvas.Font = CodeFonts.Regular;
        canvas.FontColor = PresentationTheme.CodeText;
        canvas.FontSize = Math.Clamp(rect.Width * 0.027f, 10f, 15f);
        var lineHeight = Math.Clamp(rect.Height * 0.044f, 18f, 25f);
        var y = rect.Y + rect.Height * 0.13f;
        foreach (var line in lines)
        {
            canvas.DrawString(line, rect.X + 24, y, rect.Width - 48, lineHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            y += lineHeight;
        }

        canvas.Font = CodeFonts.SemiBold;
        canvas.FontColor = PresentationTheme.CodeMetric;
        canvas.FontSize = Math.Clamp(rect.Width * 0.036f, 14f, 20f);
        canvas.DrawString($"Selected: {GetChampionName(_state.Champion)}", rect.X + 24,
            rect.Bottom - 40, rect.Width - 48, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static string GetChampionName(Champion? champion) => champion switch
    {
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "THE RACING DRIVER"
    };

    private static string GetThemeTagline(Champion? champion) => champion switch
    {
        Champion.Charioteer => "The arena awaits.",
        Champion.SpacePilot => "Course plotted. Welcome to Orbit.",
        _ => "Engines ready. Lights out."
    };
}
