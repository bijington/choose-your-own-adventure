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
    CodeReveal,
    Complete
}

public enum Champion
{
    RacingDriver,
    Charioteer,
    SpacePilot
}

public enum GameLoopStage
{
    Overview,
    FirmwareComparison,
    Running
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

    public int Health { get; set; } = 5;
    public int Experience { get; set; }
    public string CurrentLocation { get; set; } = "The Beginning";

    public void AdvanceOpening()
    {
        OpeningStage = OpeningStage switch
        {
            OpeningStage.Title => OpeningStage.Awakening,
            OpeningStage.Awakening => OpeningStage.Memory,
            OpeningStage.Memory => OpeningStage.DotNetMemory,
            OpeningStage.DotNetMemory => OpeningStage.DependencyInjection,
            OpeningStage.DependencyInjection => OpeningStage.ChooseChampion,
            OpeningStage.ChampionChosen => OpeningStage.CodeReveal,
            OpeningStage.CodeReveal => OpeningStage.Complete,
            _ => OpeningStage
        };
    }

    public void HighlightPreviousChampion() => MoveChampionHighlight(-1);

    public void HighlightNextChampion() => MoveChampionHighlight(1);

    public void HighlightChampion(Champion champion)
    {
        HighlightedChampion = champion;
    }

    public void ConfirmHighlightedChampion() => ChooseChampion(HighlightedChampion);

    public void ChooseChampion(Champion champion)
    {
        HighlightedChampion = champion;
        Champion = champion;
        OpeningStage = OpeningStage.ChampionChosen;
    }

    public void AdvanceGameLoop()
    {
        GameLoopStage = GameLoopStage switch
        {
            GameLoopStage.Overview => GameLoopStage.FirmwareComparison,
            GameLoopStage.FirmwareComparison => GameLoopStage.Running,
            _ => GameLoopStage
        };
    }

    private void MoveChampionHighlight(int direction)
    {
        var currentIndex = Array.IndexOf(Champions, HighlightedChampion);
        var nextIndex = (currentIndex + direction + Champions.Length) % Champions.Length;
        HighlightedChampion = Champions[nextIndex];
    }
}
