namespace ChooseYourOwnAdventure.Presentation;

// Display-only state: enlarging/dismissing a QR never starts voting or racing.
public sealed class ParticipationQrState
{
    private PresentationSlide? _slide;
    private bool _initialCodeShown;
    public bool IsExpanded { get; private set; }
    public bool CanShow => _slide is PresentationSlide.ChooseChampion or PresentationSlide.BoostChoice
        or PresentationSlide.AudienceRace or PresentationSlide.AfterCreditsRace;
    public void Visit(PresentationSlide slide)
    {
        if (_slide == slide) return;
        _slide = slide;
        IsExpanded = false;
        if (slide == PresentationSlide.ChooseChampion && !_initialCodeShown)
        {
            IsExpanded = true;
            _initialCodeShown = true;
        }
    }
    public void Toggle() { if (CanShow) IsExpanded = !IsExpanded; }
    public bool Dismiss()
    {
        var wasExpanded = IsExpanded;
        IsExpanded = false;
        return wasExpanded;
    }
    public void ResetSession()
    {
        _slide = null;
        IsExpanded = false;
        _initialCodeShown = false;
    }
}
