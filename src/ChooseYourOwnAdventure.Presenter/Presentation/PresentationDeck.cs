using ChooseYourOwnAdventure.Scenes;
using Microsoft.Extensions.DependencyInjection;

namespace ChooseYourOwnAdventure.Presentation;

public static class PresentationDeck
{
    private static readonly PresentationSlideDefinition[] Slides =
    [
        PresentationSlideDefinition.Create<TitleScene>(PresentationSlide.Title),
        PresentationSlideDefinition.Create<FirstDecisionScene>(PresentationSlide.FirstDecision,
            next: PresentationSlide.PrologueChapter),
        PresentationSlideDefinition.Create<AfterPartyScene>(PresentationSlide.AfterParty),
        PresentationSlideDefinition.Create<PrologueChapterScene>(PresentationSlide.PrologueChapter,
            previous: PresentationSlide.FirstDecision),
        PresentationSlideDefinition.Create<OrbitOverviewScene>(PresentationSlide.OrbitOverview,
            state => state.ResetOrbitOverview()),
        PresentationSlideDefinition.Create<InstallOrbitScene>(PresentationSlide.InstallOrbit),
        PresentationSlideDefinition.Create<GameSceneViewIntroScene>(PresentationSlide.GameSceneView),
        PresentationSlideDefinition.Create<LoadGameSceneIntroScene>(PresentationSlide.LoadGameScene),
        PresentationSlideDefinition.Create<ChampionChapterScene>(PresentationSlide.ChampionChapter),
        PresentationSlideDefinition.Create<OpeningScene>(
            PresentationSlide.ChooseChampion,
            state => state.PrepareChampionVote()),
        PresentationSlideDefinition.Create<ChosenChampionScene>(PresentationSlide.ChosenChampion,
            state => state.PrepareChampionReveal()),
        PresentationSlideDefinition.Create<InputChapterScene>(PresentationSlide.InputChapter),
        PresentationSlideDefinition.Create<InputPackageScene>(PresentationSlide.InputPackage),
        PresentationSlideDefinition.Create<GameLoopScene>(
            PresentationSlide.GameLoop,
            state => state.ResetGameLoop()),
        PresentationSlideDefinition.Create<VehicleControlScene>(PresentationSlide.VehicleControl),
        PresentationSlideDefinition.Create<SignalRChapterScene>(PresentationSlide.SignalRChapter),
        PresentationSlideDefinition.Create<SignalRTransitionScene>(PresentationSlide.SignalRTransition),
        PresentationSlideDefinition.Create<SignalRArchitectureScene>(PresentationSlide.SignalRArchitecture,
            state => state.ResetSignalRWalkthrough()),
        PresentationSlideDefinition.Create<AudienceRaceScene>(PresentationSlide.AudienceRace),
        PresentationSlideDefinition.Create<EpilogueChapterScene>(PresentationSlide.EpilogueChapter),
        PresentationSlideDefinition.Create<PodiumScene>(PresentationSlide.Podium),
        PresentationSlideDefinition.Create<RecapScene>(PresentationSlide.Recap)
    ];

    // Printed page order excludes the cover and the optional page-404 gag.
    private static readonly PresentationSlide[] StoryPages = Slides
        .Select(definition => definition.Slide)
        .Where(slide => slide is not (PresentationSlide.Title or PresentationSlide.AfterParty))
        .ToArray();

    public static int StoryPageNumber(PresentationSlide slide)
    {
        var index = Array.IndexOf(StoryPages, slide);
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(slide));
        return index + 1;
    }

    public static bool UsesChampionOffset(PresentationSlide slide)
        => StoryPageNumber(slide) >= StoryPageNumber(PresentationSlide.ChosenChampion);

    public static IServiceCollection AddChooseYourOwnAdventurePresentation(
        this IServiceCollection services)
    {
        services.AddSingleton<BookScene>();

        foreach (var definition in Slides)
        {
            services.AddTransient(definition.SceneType);
        }

        services.AddSingleton<IReadOnlyList<PresentationSlideDefinition>>(Slides);
        services.AddSingleton<IPresentationNavigator, PresentationNavigator>();

        return services;
    }
}
