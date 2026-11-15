namespace ChooseYourOwnAdventure.Scenes;

/// <summary>The vote's chosen character, introduced within Chapter One.</summary>
public sealed class ChosenChampionScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.ChosenChampion, "THE AUDIENCE HAS CHOSEN", "OUR CHAMPION",
    string.Empty, showChampion: true);
