using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Orbit.Engine;
using Orbit.Input;

namespace ChooseYourOwnAdventure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseOrbitEngine()
            .UseOrbitGameController(o => {})
            .UseOrbitKeyboard()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("CascadiaCode-VariableFont_wght.ttf", "CascadiaCode");
            });

        builder.Services.AddSingleton<AdventureState>();
        builder.Services.AddSingleton<AdventureInputRouter>();
        builder.Services.AddSingleton(GameControllerManager.Current);
        builder.Services.AddSingleton(KeyboardManager.Current);
        builder.Services.AddSingleton<OrbitInputBridge>();
        builder.Services.AddSingleton(new LiveAudienceOptions());
        builder.Services.AddSingleton<LiveVoteState>();
        builder.Services.AddSingleton<LiveRaceState>();
        builder.Services.AddSingleton<AudienceVoteClient>();
        builder.Services.AddSingleton<QrCodeRenderer>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddChooseYourOwnAdventurePresentation();

        return builder.Build();
    }
}
