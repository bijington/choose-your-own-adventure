using ChooseYourOwnAdventure.PresenterNotes;

namespace ChooseYourOwnAdventure;

public partial class App : Application
{
    private readonly MainPage _mainPage;
    private readonly PresenterNotesWindow _notes;
    private Window? _presentation;

    public App(MainPage mainPage, PresenterNotesWindow notes)
    {
        InitializeComponent();
        _mainPage = mainPage;
        _notes = notes;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // OS-created secondary windows must never reuse the Orbit page.
        if (_presentation is not null) return _notes.CreateNotesWindow();
        _presentation = new Window(_mainPage) { Title = "Choose Your Own Adventure — Audience" };
        _notes.Attach(_presentation);
        return _presentation;
    }
}
