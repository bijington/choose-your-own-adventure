using ChooseYourOwnAdventure.Live;

namespace ChooseYourOwnAdventure.Input;

/// <summary>Resolves forward navigation without initiating live interactions.</summary>
public static class PresentationNavigation
{
    public static AdventureAction? ResolveNext(PresentationSlide slide, AdventureState state,
        VoteSnapshot vote, RaceSnapshot race, bool connected) => slide switch
    {
        PresentationSlide.ChooseChampion => state.LocalVoteWinner is not null
            || (connected && vote.Phase == VotePhase.Revealed && vote.RevealedChampion is not null)
                ? AdventureAction.Confirm : null,
        PresentationSlide.AudienceRace => race.Phase == RacePhase.Finished ? AdventureAction.Confirm : null,
        PresentationSlide.ChosenChampion => AdventureAction.Confirm,
        PresentationSlide.SignalRArchitecture => AdventureAction.Confirm,
        PresentationSlide.OrbitOverview => AdventureAction.Confirm,
        PresentationSlide.GameLoop => AdventureAction.Right,
        _ => AdventureAction.Next
    };
}
