using Adventure.Contracts;
using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.PresenterNotes;
using Orbit.Engine;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using ChooseYourOwnAdventure.Scenes;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
    private static readonly TimeSpan VoteRevealDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan VoteLockInDuration = TimeSpan.FromSeconds(3);

    private readonly IPresentationNavigator _presentationNavigator;
    private readonly AdventureState _adventureState;
    private readonly AdventureInputRouter _inputRouter;
    private readonly OrbitInputBridge _orbitInputBridge;
    private readonly AudienceVoteClient _audienceVoteClient;
    private readonly LiveVoteState _liveVoteState;
    private readonly LiveRaceState _liveRaceState;
    private readonly LiveAudienceOptions _liveOptions;

    private readonly PresenterNotesWindow _notes;
    private bool _hasStarted;
    private readonly bool _isMobile;
    private readonly IReadOnlyList<PresentationSlideDefinition> _slides;

    public MainPage(
        IPresentationNavigator presentationNavigator,
        AdventureState adventureState,
        AdventureInputRouter inputRouter,
        OrbitInputBridge orbitInputBridge,
        AudienceVoteClient audienceVoteClient,
        LiveVoteState liveVoteState,
        LiveRaceState liveRaceState,
        LiveAudienceOptions liveOptions,
        QrCodeRenderer participationQr,
        PresenterNotesWindow notes,
        IReadOnlyList<PresentationSlideDefinition> slides)
    {
        InitializeComponent();
        _notes = notes;
        _slides = slides;
        _liveOptions = liveOptions;
        ParticipationCode.Configure(participationQr, liveOptions);
        liveOptions.SessionId = Preferences.Default.Get("AudienceSessionId", liveOptions.SessionId);
        _isMobile = DeviceInfo.Current.Idiom == DeviceIdiom.Phone || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet;
        adventureState.IsRehearsing = _isMobile && Preferences.Default.Get("MobilePracticeMode", true);
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        var presenterMenu = new MenuBarItem { Text = "Presenter" };
        presenterMenu.Add(new MenuFlyoutItem { Text = "Show Presenter Notes", Command = new Command(_notes.Show) });
        presenterMenu.Add(PresenterMenuItems.CreateSlideNavigation(slides, slide =>
        {
            if (_hasStarted) presentationNavigator.GoTo(slide);
        }));
        presenterMenu.Add(new MenuFlyoutItem { Text = "New Audience Session", Command = new Command(async () => await CreateAudienceSessionAsync()) });
        presenterMenu.Add(new MenuFlyoutItem { Text = "Practice Options", Command = new Command(async () => await ShowPracticeOptionsAsync()) });
        MenuBarItems.Add(presenterMenu);
#endif
        _presentationNavigator = presentationNavigator;
        _adventureState = adventureState;
        _inputRouter = inputRouter;
        _orbitInputBridge = orbitInputBridge;
        _audienceVoteClient = audienceVoteClient;
        _liveVoteState = liveVoteState;
        _liveRaceState = liveRaceState;
        UpdatePracticeButton();

        _inputRouter.ActionTriggered += OnAdventureAction;
    }

    private void SyncParticipationQr()
    {
        if (!_hasStarted || _presentationNavigator.IsTurning)
        {
            ParticipationCode.IsVisible = false;
            EnlargeQrButton.IsVisible = false;
            return;
        }
        var qr = _adventureState.ParticipationQr;
        qr.Visit(_presentationNavigator.Current);
        ParticipationCode.IsVisible = qr.IsExpanded;
        EnlargeQrButton.IsVisible = qr.CanShow && !qr.IsExpanded;
        if (qr.IsExpanded) ParticipationCode.Refresh(PresenterCueFactory.TitleFor(_presentationNavigator.Current));
    }
    private void OnToggleParticipationQr(object? sender, EventArgs e)
    {
        _adventureState.ParticipationQr.Toggle();
        SyncParticipationQr();
    }
    private void OnDismissParticipationQr(object? sender, EventArgs e)
    {
        _adventureState.ParticipationQr.Dismiss();
        SyncParticipationQr();
    }

    private void OnAnimationContinueRequested(object? sender, EventArgs e) => _inputRouter.Trigger(AdventureAction.Confirm);

    private async void OnPracticeOptionsClicked(object? sender, EventArgs e) => await ShowPracticeOptionsAsync();

    private void UpdatePracticeButton()
    {
        PracticeButton.IsVisible = _isMobile || _adventureState.IsRehearsing;
        PracticeButton.Text = _adventureState.IsRehearsing ? "Practice · options" : "Live · options";
    }

    private async Task ShowPracticeOptionsAsync()
    {
        if (!_hasStarted || _presentationNavigator.IsTurning) return;
        var choices = _adventureState.IsRehearsing
            ? new[] { "New audience session", "Choose a champion", "Skip the audience race", "Go to slide", "Use live audience" }
            : new[] { "New audience session", "Practise without the audience", "Go to slide" };
        var choice = await DisplayActionSheet(_adventureState.IsRehearsing ? "Practice mode" : "Live audience mode",
            "Cancel", null, choices);
        switch (choice)
        {
            case "New audience session":
                await CreateAudienceSessionAsync();
                break;
            case "Practise without the audience":
                if (_audienceVoteClient.IsConnected) await _audienceVoteClient.StopSteeringRaceAsync();
                _adventureState.IsRehearsing = true;
                _adventureState.ResetLocalVote();
                if (_isMobile) Preferences.Default.Set("MobilePracticeMode", true);
                UpdatePracticeButton();
                await _audienceVoteClient.PauseAsync();
                break;
            case "Use live audience":
                _adventureState.IsRehearsing = false;
                _adventureState.ResetLocalVote();
                if (_isMobile) Preferences.Default.Set("MobilePracticeMode", false);
                UpdatePracticeButton();
                await _audienceVoteClient.StartAsync();
                break;
            case "Choose a champion":
                var champion = await DisplayActionSheet("Practise with…", "Cancel", null,
                    "The Driver", "The Charioteer", "The Space Pilot");
                Champion? selected = champion switch
                {
                    "The Driver" => Champion.RacingDriver,
                    "The Charioteer" => Champion.Charioteer,
                    "The Space Pilot" => Champion.SpacePilot,
                    _ => null
                };
                if (selected is { } local)
                {
                    _adventureState.ResetLocalVote();
                    _adventureState.ChooseChampion(local);
                    _presentationNavigator.GoTo(PresentationSlide.ChosenChampion);
                }
                break;
            case "Skip the audience race":
                _presentationNavigator.GoTo(PresentationSlide.Podium);
                break;
            case "Go to slide":
                var title = await DisplayActionSheet("Go to slide", "Cancel", null,
                    _slides.Select(slide => PresenterCueFactory.TitleFor(slide.Slide)).ToArray());
                var destination = _slides.FirstOrDefault(slide => PresenterCueFactory.TitleFor(slide.Slide) == title);
                if (destination is not null) _presentationNavigator.GoTo(destination.Slide);
                break;
        }
    }

    private async Task CreateAudienceSessionAsync()
    {
        if (!_hasStarted || _presentationNavigator.IsTurning) return;
        var session = await _audienceVoteClient.CreateSessionAsync();
        Preferences.Default.Set("AudienceSessionId", session);
        _adventureState.PrepareChampionVote();
        await DisplayAlert("New audience session", $"{session}\n\n{_liveOptions.JoinUrl}\n\nThe vote and race start fresh. Ask everyone to scan the new QR code. This session is remembered on this device."
            + (_adventureState.IsRehearsing ? "\n\nPractice mode is still on; select Use live audience when ready." : ""), "OK");
    }

    private void OnGameViewLoaded(object? sender, EventArgs e)
    {
        if (_hasStarted)
        {
            return;
        }

        _presentationNavigator.Start(GameView);
        _hasStarted = true;
        var animationTimer = Dispatcher.CreateTimer();
        animationTimer.Interval = TimeSpan.FromMilliseconds(100);
        var wasInBonus = false;
        animationTimer.Tick += (_, _) =>
        {
            SyncParticipationQr();
            var inBonus = _presentationNavigator.Current == PresentationSlide.AfterCreditsRace;
            if (wasInBonus && !inBonus && _audienceVoteClient.IsConnected) _ = _audienceVoteClient.StopSteeringRaceAsync();
            wasInBonus = inBonus;
            AnimationShowcase.Margin = new Thickness(GameView.Width * .54, GameView.Height * .25,
                GameView.Width * .065, GameView.Height * .17);
            AnimationShowcase.IsVisible = _presentationNavigator.Current == PresentationSlide.MauiAnimations
                && !_presentationNavigator.IsTurning;
        };
        Loaded += (_, _) => animationTimer.Start();
        Unloaded += (_, _) =>
        {
            animationTimer.Stop();
            AnimationShowcase.IsVisible = false;
        };
        animationTimer.Start();
        _notes.StartSession();
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        // The audience scene is already running; opening notes cannot block startup.
        Dispatcher.Dispatch(_notes.Show);
#endif

        // Input/network discovery must never block the presentation rendering.
        _ = _orbitInputBridge.StartAsync();
        _ = _audienceVoteClient.StartAsync();
    }

    private void OnGameViewTapped(object? sender, TappedEventArgs e)
    {
        if (_presentationNavigator.IsTurning) return;

        var position = e.GetPosition(GameView);
        if (position is null)
        {
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.AudienceRace && _adventureState.IsRehearsing)
        {
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d ? AdventureAction.Previous : AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.AfterCreditsRace)
        {
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2 ? AdventureAction.Cancel : AdventureAction.Confirm);
            return;
        }
        if (_presentationNavigator.Current == PresentationSlide.BoostChoice)
        {
            if (position.Value.X < GameView.Width / 2d) _presentationNavigator.Previous();
            else
            {
                _adventureState.HighlightedBoostMode = position.Value.Y < GameView.Height * .48
                    ? BoostMode.Steady : BoostMode.ChargeAndRelease;
                _inputRouter.Trigger(AdventureAction.Confirm);
            }
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.FirstDecision)
        {
            if (position.Value.X < GameView.Width / 2d)
                _presentationNavigator.Previous();
            else if (FirstDecisionScene.IsAfterPartyChoice(
                new Microsoft.Maui.Graphics.RectF(0, 0, (float)GameView.Width, (float)GameView.Height),
                new Microsoft.Maui.Graphics.PointF((float)position.Value.X, (float)position.Value.Y)))
                _presentationNavigator.GoTo(PresentationSlide.AfterParty);
            else
                _presentationNavigator.Next();
            return;
        }

        if (_presentationNavigator.Current is PresentationSlide.ChosenChampion
            or PresentationSlide.SignalRArchitecture or PresentationSlide.OrbitOverview or PresentationSlide.InputPackage)
        {
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d
                ? AdventureAction.Previous : AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.ChooseChampion
            && position.Value.X < GameView.Width / 2d)
        {
            _presentationNavigator.Previous();
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.ChooseChampion && _adventureState.OpeningStage == OpeningStage.ChooseChampion)
        {
            var chosen = ChampionSelectionLayout.HitTest(
                new Microsoft.Maui.Graphics.RectF(0, 0, (float)GameView.Width, (float)GameView.Height),
                new Microsoft.Maui.Graphics.PointF((float)position.Value.X, (float)position.Value.Y));
            if (chosen is { } champion) _adventureState.HighlightChampion(champion);
            _inputRouter.Trigger(AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.GameLoop)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.75d
                && position.Value.Y >= GameView.Height * 0.88d;
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d
                ? AdventureAction.Cancel : isNextArea ? AdventureAction.Right : AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.VehicleControl)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.72d
                && position.Value.Y >= GameView.Height * 0.84d;

            if (isNextArea)
            {
                _presentationNavigator.GoTo(PresentationSlide.BoostChoice);
            }
            else
            {
                _ = _inputRouter.PulseAsync(AdventureAction.Confirm, TimeSpan.FromMilliseconds(400));
            }
            return;
        }

        // Each facing page is a navigation target on ordinary spreads.
        // The cover and interactive spreads retain their own touch controls.
        if (_presentationNavigator.Current is not (PresentationSlide.Title
                or PresentationSlide.ChooseChampion or PresentationSlide.AudienceRace)
            && position.Value.X < GameView.Width / 2d)
        {
            _presentationNavigator.Previous();
            return;
        }

        _inputRouter.Trigger(AdventureAction.Confirm);
    }

    private async void OnAdventureAction(object? sender, AdventureAction action)
    {
        if (_presentationNavigator.IsTurning) return;
        SyncParticipationQr();
        if (_adventureState.ParticipationQr.IsExpanded)
        {
            if (action is AdventureAction.Confirm or AdventureAction.Next or AdventureAction.Previous or AdventureAction.Cancel)
                OnDismissParticipationQr(this, EventArgs.Empty);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.AfterCreditsRace)
        {
            var bonus = _audienceVoteClient.Steering.Snapshot;
            if (action is AdventureAction.Cancel or AdventureAction.Previous)
            {
                if (_audienceVoteClient.IsConnected) await _audienceVoteClient.StopSteeringRaceAsync();
                _presentationNavigator.GoTo(PresentationSlide.Recap);
            }
            else if (action == AdventureAction.Confirm)
            {
                if (!_audienceVoteClient.IsConnected)
                    await DisplayAlert("Bonus race", "This optional race needs a live connection and registered champion teams. Practice mode shows the briefing only.", "OK");
                else if (bonus.Phase == SteeringPhase.Inactive)
                {
                    if (_liveVoteState.Snapshot.Total == 0)
                        await DisplayAlert("Bonus race", "The audience must choose champion teams before preparing this race.", "OK");
                    else await _audienceVoteClient.PrepareSteeringRaceAsync();
                }
                else if (bonus.Phase == SteeringPhase.Ready) await _audienceVoteClient.StartSteeringRaceAsync();
                else if (bonus.Phase == SteeringPhase.Finished)
                {
                    await _audienceVoteClient.StopSteeringRaceAsync();
                    _presentationNavigator.GoTo(PresentationSlide.Recap);
                }
            }
            return;
        }

        // Navigation never starts or resets a vote/race. Confirmation owns those commands.
        if (action == AdventureAction.Previous)
        {
            if (_presentationNavigator.Current == PresentationSlide.InputPackage
                && _adventureState.PreviousControllerStage()) return;
            if (_presentationNavigator.Current == PresentationSlide.OrbitOverview
                && _adventureState.PreviousOrbitOverviewStage()) return;
            if (_presentationNavigator.Current == PresentationSlide.SignalRArchitecture
                && _adventureState.PreviousSignalRStage()) return;
            if (_presentationNavigator.Current == PresentationSlide.ChosenChampion
                && _adventureState.PreviousChampionRendering())
                return;
            else
                _presentationNavigator.Previous();
            return;
        }
        if (action == AdventureAction.Next)
        {
            var resolved = PresentationNavigation.ResolveNext(_presentationNavigator.Current,
                _adventureState, _liveVoteState.Snapshot, _liveRaceState.Snapshot, _audienceVoteClient.IsConnected);
            if (resolved is null) return;
            if (resolved == AdventureAction.Next)
            {
                _presentationNavigator.Next();
                return;
            }
            action = resolved.Value;
        }

        switch (_presentationNavigator.Current)
        {
            case PresentationSlide.BoostChoice:
                var boostVote = _liveRaceState.Snapshot;
                if (action is AdventureAction.Up or AdventureAction.Left)
                    _adventureState.HighlightedBoostMode = BoostMode.Steady;
                else if (action is AdventureAction.Down or AdventureAction.Right)
                    _adventureState.HighlightedBoostMode = BoostMode.ChargeAndRelease;
                else if (action == AdventureAction.Confirm)
                {
                    if (boostVote.BoostVotePhase == VotePhase.Closed) _presentationNavigator.Next();
                    else if (!_audienceVoteClient.IsConnected)
                        _liveRaceState.Update(boostVote with { Mode = _adventureState.HighlightedBoostMode, BoostVotePhase = VotePhase.Closed });
                    else if (boostVote.BoostVotePhase == VotePhase.Open) await _audienceVoteClient.CloseBoostVotingAsync();
                    else await _audienceVoteClient.OpenBoostVotingAsync();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (_audienceVoteClient.IsConnected) await _audienceVoteClient.OpenBoostVotingAsync();
                    else _liveRaceState.Update(boostVote with { BoostVotePhase = VotePhase.Ready });
                }
                return;

            case PresentationSlide.InputPackage:
                if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceControllerStage()) _presentationNavigator.Next();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousControllerStage()) _presentationNavigator.Previous();
                }
                return;
            case PresentationSlide.FirstDecision:
                if (action == AdventureAction.Cancel)
                    _presentationNavigator.GoTo(PresentationSlide.AfterParty);
                else if (action == AdventureAction.Confirm)
                    _presentationNavigator.Next();
                return;

            case PresentationSlide.ChooseChampion:
                await HandleChampionVoteAsync(action);
                return;

            case PresentationSlide.ChosenChampion:
                if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousChampionRendering())
                        _presentationNavigator.Previous();
                }
                else if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceChampionRendering())
                        _presentationNavigator.Next();
                }
                return;

            case PresentationSlide.GameLoop:
                HandleGameLoop(action);
                return;

            case PresentationSlide.OrbitOverview:
                if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceOrbitOverviewStage()) _presentationNavigator.Next();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousOrbitOverviewStage()) _presentationNavigator.Previous();
                }
                return;

            case PresentationSlide.SignalRArchitecture:
                if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceSignalRStage()) _presentationNavigator.Next();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousSignalRStage()) _presentationNavigator.Previous();
                }
                return;

            case PresentationSlide.VehicleControl:
                if (action == AdventureAction.Up)
                {
                    _presentationNavigator.GoTo(PresentationSlide.BoostChoice);
                }
                return;

            case PresentationSlide.AudienceRace:
                await HandleAudienceRaceAsync(action);
                return;
        }

        if (action == AdventureAction.Cancel)
        {
            _presentationNavigator.Previous();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        _presentationNavigator.Next();
    }

    private async Task HandleChampionVoteAsync(AdventureAction action)
    {
        if (action == AdventureAction.Cancel)
        {
            _adventureState.ResetLocalVote();
            if (_audienceVoteClient.IsConnected) await _audienceVoteClient.ResetVotingAsync();
            return;
        }

        if (action is AdventureAction.Left or AdventureAction.Up)
        {
            _adventureState.HighlightPreviousChampion();
            return;
        }

        if (action is AdventureAction.Right or AdventureAction.Down)
        {
            _adventureState.HighlightNextChampion();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        if (_adventureState.LocalVoteWinner is not null || !_audienceVoteClient.IsConnected)
        {
            if (_adventureState.LocalVoteWinner is not { } localWinner)
            {
                _adventureState.RevealLocalVote();
            }
            else if (DateTimeOffset.UtcNow - _adventureState.LocalVoteRevealedAt >= VoteRevealDuration)
            {
                _adventureState.ChooseChampion(localWinner);
                _presentationNavigator.GoTo(PresentationSlide.ChosenChampion);
            }
            return;
        }

        var vote = _liveVoteState.Snapshot;
        switch (vote.Phase)
        {
            case VotePhase.Ready:
                await _audienceVoteClient.OpenVotingAsync();
                break;
            case VotePhase.Open:
                await _audienceVoteClient.CloseVotingAsync();
                break;
            case VotePhase.Closed:
                if (_liveVoteState.PhaseElapsed >= VoteLockInDuration)
                {
                    await _audienceVoteClient.RevealWinnerAsync();
                }
                break;
            case VotePhase.Revealed when vote.RevealedChampion is { } winner:
                if (_liveVoteState.PhaseElapsed < VoteRevealDuration) break;
                _adventureState.ChooseChampion(winner);
                _presentationNavigator.GoTo(PresentationSlide.ChosenChampion);
                break;
            case VotePhase.Revealed:
                await _audienceVoteClient.OpenVotingAsync();
                break;
        }
    }

    private void HandleGameLoop(AdventureAction action)
    {
        if (action == AdventureAction.Confirm)
        {
            _adventureState.AdvanceLoopStation();
            return;
        }
        if (action is AdventureAction.Left or AdventureAction.Cancel)
        {
            if (_adventureState.GameLoopStation == LoopStation.Input)
                _presentationNavigator.Previous();
            else
                _adventureState.PreviousLoopStation();
            return;
        }
        if (action == AdventureAction.Right)
            _presentationNavigator.Next();
    }

    private async Task HandleAudienceRaceAsync(AdventureAction action)
    {
        if (_adventureState.IsRehearsing)
        {
            if (action == AdventureAction.Confirm) _presentationNavigator.GoTo(PresentationSlide.Podium);
            else if (action == AdventureAction.Cancel) _presentationNavigator.Previous();
            return;
        }
        if (action == AdventureAction.Cancel)
        {
            await _audienceVoteClient.ResetRaceAsync();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        var race = _liveRaceState.Snapshot;
        if (race.Phase == RacePhase.Ready)
        {
            await _audienceVoteClient.StartRaceAsync();
        }
        else if (race.Phase == RacePhase.Finished)
        {
            _presentationNavigator.GoTo(PresentationSlide.Podium);
        }
    }

}
