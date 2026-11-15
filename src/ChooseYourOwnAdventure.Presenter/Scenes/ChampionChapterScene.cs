namespace ChooseYourOwnAdventure.Scenes;

public sealed class ChampionChapterScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.ChampionChapter, "CHAPTER ONE", "MEET OUR CHAMPION",
    "A little self-discovery. A few lines of C#.\nLet’s find out what our champion is made of.");
