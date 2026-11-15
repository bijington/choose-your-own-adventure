using ChooseYourOwnAdventure.Scenes;
using Microsoft.Extensions.DependencyInjection;

namespace ChooseYourOwnAdventure.Presentation;

public static class PresentationDeck
{
    private static readonly PresentationSlideDefinition[] Slides =
    [
        PresentationSlideDefinition.Create<TitleScene>(PresentationSlide.Title),
        PresentationSlideDefinition.Create<OrbitOverviewScene>(PresentationSlide.OrbitOverview),
        PresentationSlideDefinition.Create<InstallOrbitScene>(PresentationSlide.InstallOrbit),
        PresentationSlideDefinition.Create<GameSceneViewIntroScene>(PresentationSlide.GameSceneView),
        PresentationSlideDefinition.Create<LoadGameSceneIntroScene>(PresentationSlide.LoadGameScene),
        PresentationSlideDefinition.Create<OpeningScene>(
            PresentationSlide.ChooseChampion,
            state => state.PrepareChampionVote()),
        PresentationSlideDefinition.Create<AddGameObjectScene>(PresentationSlide.AddGameObject),
        PresentationSlideDefinition.Create<ChampionRevealScene>(
            PresentationSlide.ChampionReveal,
            state => state.PrepareChampionReveal()),
        PresentationSlideDefinition.Create<InputPackageScene>(PresentationSlide.InputPackage),
        PresentationSlideDefinition.Create<GameLoopScene>(
            PresentationSlide.GameLoop,
            state => state.ResetGameLoop()),
        PresentationSlideDefinition.Create<VehicleControlScene>(PresentationSlide.VehicleControl),
        PresentationSlideDefinition.Create<SignalRTransitionScene>(PresentationSlide.SignalRTransition),
        PresentationSlideDefinition.Create<SignalRArchitectureScene>(PresentationSlide.SignalRArchitecture),
        PresentationSlideDefinition.Create<AudienceRaceScene>(PresentationSlide.AudienceRace),
        PresentationSlideDefinition.Create<PodiumScene>(PresentationSlide.Podium),
        PresentationSlideDefinition.Create<RecapScene>(PresentationSlide.Recap)
    ];

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
