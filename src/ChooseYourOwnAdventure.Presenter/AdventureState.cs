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
    public float CodeRevealTransitionProgress { get; private set; }

    public int Health { get; set; } = 5;
    public int Experience { get; set; }
    public string CurrentLocation { get; set; } = "The Beginning";


    public void PrepareChampionVote()
    {
        OpeningStage = OpeningStage.ChooseChampion;
        CodeRevealTransitionProgress = 0f;
    }

    public void PrepareChampionReveal()
    {
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

    private void MoveChampionHighlight(int direction)
    {
        var currentIndex = Array.IndexOf(Champions, HighlightedChampion);
        var nextIndex = (currentIndex + direction + Champions.Length) % Champions.Length;
        HighlightedChampion = Champions[nextIndex];
    }
}
