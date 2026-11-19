namespace ChooseYourOwnAdventure;

public enum OpeningStage
{
    Title,
    Awakening,
    Memory,
    DotNetMemory,
    DependencyInjection,
    ChooseChampion,
    ChampionChosen,
    ChampionTransition,
    CodeReveal,
    Complete
}

public enum Champion
{
    RacingDriver = 20,
    Charioteer = 50,
    SpacePilot = 80
}

public enum LoopStation
{
    Input = 0,
    Update = 1,
    Render = 2,
    Wait = 3
}

public enum GameLoopStage
{
    Overview,
    FirmwareComparison
}

public enum SignalRStage
{
    Browser,
    Hub,
    Broadcast,
    Presenter
}

public enum OrbitOverviewStage
{
    Maui,
    Graphics,
    Orbit,
    Game
}

public sealed class AdventureState
{
    private static readonly Champion[] Champions =
    [
        ChooseYourOwnAdventure.Champion.RacingDriver,
        ChooseYourOwnAdventure.Champion.Charioteer,
        ChooseYourOwnAdventure.Champion.SpacePilot
    ];

    public OpeningStage OpeningStage { get; private set; } = OpeningStage.Title;
    public Champion? Champion { get; private set; }
    public Champion HighlightedChampion { get; private set; } = ChooseYourOwnAdventure.Champion.RacingDriver;
    public GameLoopStage GameLoopStage { get; private set; } = GameLoopStage.Overview;
    public LoopStation GameLoopStation { get; private set; } = LoopStation.Input;
    public SignalRStage SignalRStage { get; private set; }
    public OrbitOverviewStage OrbitOverviewStage { get; private set; }
    public float CodeRevealTransitionProgress { get; private set; }

    public int Health { get; set; } = 5;
    public int Experience { get; set; }
    public string CurrentLocation { get; set; } = "The Beginning";


    public Champion? LocalVoteWinner { get; private set; }
    public DateTimeOffset LocalVoteRevealedAt { get; private set; }

    public void RevealLocalVote()
    {
        LocalVoteWinner = HighlightedChampion;
        LocalVoteRevealedAt = DateTimeOffset.UtcNow;
    }

    public void ResetLocalVote() => LocalVoteWinner = null;

    public void PrepareChampionVote()
    {
        OpeningStage = OpeningStage.ChooseChampion;
        CodeRevealTransitionProgress = 0f;
    }

    public bool ChampionRenderingVisible { get; private set; }
    public void ShowChampionRendering() => ChampionRenderingVisible = true;
    public void HideChampionRendering() => ChampionRenderingVisible = false;

    public void PrepareChampionReveal()
    {
        ChampionRenderingVisible = false;
        OpeningStage = OpeningStage.ChampionChosen;
        CodeRevealTransitionProgress = 0f;
    }

    public void ResetGameLoop()
    {
        GameLoopStage = GameLoopStage.Overview;
        GameLoopStation = LoopStation.Input;
    }

    public void AdvanceOpening()
    {
        OpeningStage = OpeningStage switch
        {
            OpeningStage.Title => OpeningStage.Awakening,
            OpeningStage.Awakening => OpeningStage.Memory,
            OpeningStage.Memory => OpeningStage.DotNetMemory,
            OpeningStage.DotNetMemory => OpeningStage.DependencyInjection,
            OpeningStage.DependencyInjection => OpeningStage.ChooseChampion,
            OpeningStage.ChampionChosen => OpeningStage.ChampionTransition,
            OpeningStage.CodeReveal => OpeningStage.Complete,
            _ => OpeningStage
        };
    }


    public void SetCodeRevealTransitionProgress(float progress)
    {
        CodeRevealTransitionProgress = Math.Clamp(progress, 0f, 1f);
    }

    public void CompleteCodeRevealTransition()
    {
        CodeRevealTransitionProgress = 1f;
        OpeningStage = OpeningStage.CodeReveal;
    }

    public void HighlightPreviousChampion() => MoveChampionHighlight(-1);
    public void HighlightNextChampion() => MoveChampionHighlight(1);
    public void HighlightChampion(Champion champion) => HighlightedChampion = champion;
    public void ConfirmHighlightedChampion() => ChooseChampion(HighlightedChampion);

    public void ChooseChampion(Champion champion)
    {
        HighlightedChampion = champion;
        Champion = champion;
        CodeRevealTransitionProgress = 0f;
        OpeningStage = OpeningStage.ChampionChosen;
    }

    public void AdvanceLoopStation() =>
        GameLoopStation = (LoopStation)(((int)GameLoopStation + 1) % 4);

    public void PreviousLoopStation() =>
        GameLoopStation = (LoopStation)(((int)GameLoopStation + 3) % 4);

    public void ShowFirmwareComparison() =>
        GameLoopStage = GameLoopStage.FirmwareComparison;

    public void ShowGameLoopOverview() => GameLoopStage = GameLoopStage.Overview;

    public void ResetSignalRWalkthrough() => SignalRStage = SignalRStage.Browser;

    public void ResetOrbitOverview() => OrbitOverviewStage = OrbitOverviewStage.Maui;

    public bool AdvanceOrbitOverviewStage()
    {
        if (OrbitOverviewStage == OrbitOverviewStage.Game) return false;
        OrbitOverviewStage++;
        return true;
    }

    public bool PreviousOrbitOverviewStage()
    {
        if (OrbitOverviewStage == OrbitOverviewStage.Maui) return false;
        OrbitOverviewStage--;
        return true;
    }

    public bool AdvanceSignalRStage()
    {
        if (SignalRStage == SignalRStage.Presenter) return false;
        SignalRStage++;
        return true;
    }

    public bool PreviousSignalRStage()
    {
        if (SignalRStage == SignalRStage.Browser) return false;
        SignalRStage--;
        return true;
    }

    private void MoveChampionHighlight(int direction)
    {
        var currentIndex = Array.IndexOf(Champions, HighlightedChampion);
        var nextIndex = (currentIndex + direction + Champions.Length) % Champions.Length;
        HighlightedChampion = Champions[nextIndex];
    }
}
