namespace ChooseYourOwnAdventure.Scenes;

public sealed class PrologueChapterScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.PrologueChapter, "PROLOGUE", "DISCOVERING ORBIT",
    "Before we choose a path, let’s discover the engine that brings this world to life.");

public sealed class InputChapterScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.InputChapter, "CHAPTER TWO", "TAKE CONTROL",
    "We know how their world runs.\nNow let’s put the controls in your hands.");

public sealed class SignalRChapterScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.SignalRChapter, "CHAPTER THREE", "AN ADVENTURE TOGETHER",
    "One champion. An entire room of players.\nLet’s turn the audience into the input.");

public sealed class EpilogueChapterScene(AdventureState state) : ChapterOpeningScene(state,
    PresentationSlide.EpilogueChapter, "EPILOGUE", "THE JOURNEY HOME",
    "Every race has a finish.\nEvery adventure leaves us with something to take home.");
