namespace ChooseYourOwnAdventure;

public enum OpeningStage
{
    Title,
    Awakening,
    Memory,
    DotNetMemory,
    DependencyInjection,
    ChooseClass,
    ClassChosen,
    CodeReveal,
    Complete
}

public enum PlayerClass
{
    MauiWarrior,
    AsyncWizard,
    XamlRanger
}

public sealed class AdventureState
{
    public OpeningStage OpeningStage { get; private set; } = OpeningStage.Title;
    public PlayerClass? PlayerClass { get; private set; }

    public int Health { get; set; } = 5;
    public int Experience { get; set; }
    public string CurrentLocation { get; set; } = "The Beginning";

    public void Advance()
    {
        OpeningStage = OpeningStage switch
        {
            OpeningStage.Title => OpeningStage.Awakening,
            OpeningStage.Awakening => OpeningStage.Memory,
            OpeningStage.Memory => OpeningStage.DotNetMemory,
            OpeningStage.DotNetMemory => OpeningStage.DependencyInjection,
            OpeningStage.DependencyInjection => OpeningStage.ChooseClass,
            OpeningStage.ClassChosen => OpeningStage.CodeReveal,
            OpeningStage.CodeReveal => OpeningStage.Complete,
            _ => OpeningStage
        };
    }

    public void ChooseClass(PlayerClass playerClass)
    {
        PlayerClass = playerClass;
        OpeningStage = OpeningStage.ClassChosen;
    }
}
