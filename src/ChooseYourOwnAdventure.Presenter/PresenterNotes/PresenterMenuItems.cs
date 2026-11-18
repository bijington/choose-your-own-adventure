using ChooseYourOwnAdventure.Presentation;

namespace ChooseYourOwnAdventure.PresenterNotes;

public static class PresenterMenuItems
{
    public static MenuFlyoutSubItem CreateSlideNavigation(
        IReadOnlyList<PresentationSlideDefinition> slides, Action<PresentationSlide> navigate)
    {
        var menu = new MenuFlyoutSubItem { Text = "Go to Slide" };
        foreach (var definition in slides)
        {
            var slide = definition.Slide;
            menu.Add(new MenuFlyoutItem
            {
                Text = PresenterCueFactory.TitleFor(slide),
                Command = new Command(() => navigate(slide))
            });
        }
        return menu;
    }
}
