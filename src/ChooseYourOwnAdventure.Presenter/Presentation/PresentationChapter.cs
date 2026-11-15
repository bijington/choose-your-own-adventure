namespace ChooseYourOwnAdventure.Presentation;

public static class PresentationChapter
{
    public static string? LabelFor(PresentationSlide slide) => slide switch
    {
        PresentationSlide.OrbitOverview or PresentationSlide.InstallOrbit
            or PresentationSlide.GameSceneView or PresentationSlide.LoadGameScene
            => "PROLOGUE · DISCOVERING ORBIT",
        PresentationSlide.ChooseChampion or PresentationSlide.AddGameObject
            or PresentationSlide.ChampionReveal => "CHAPTER ONE · MEET OUR CHAMPION",
        PresentationSlide.InputPackage or PresentationSlide.GameLoop
            or PresentationSlide.VehicleControl => "CHAPTER TWO · TAKE CONTROL",
        PresentationSlide.SignalRTransition or PresentationSlide.SignalRArchitecture
            or PresentationSlide.AudienceRace => "CHAPTER THREE · AN ADVENTURE TOGETHER",
        PresentationSlide.Podium or PresentationSlide.Recap => "EPILOGUE · THE JOURNEY HOME",
        _ => null
    };
}
