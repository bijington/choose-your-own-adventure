using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.PresenterNotes;
using Orbit.Engine;
using Orbit.Input;
using Microsoft.Extensions.Configuration;

namespace ChooseYourOwnAdventure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

#if DEBUG
        const string? configurationEnvironment = "Development";
#else
        const string? configurationEnvironment = null;
#endif
#if WINDOWS
        builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            optional: false, reloadOnChange: false);
        if (configurationEnvironment is not null)
        {
            builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{configurationEnvironment}.json"),
                optional: true, reloadOnChange: false);
        }
#else
        builder.Configuration.AddJsonPlatformBundle(configurationEnvironment);
#endif

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
        builder.Services.AddSingleton(LiveAudienceOptions.FromConfiguration(builder.Configuration));
        builder.Services.AddSingleton(TalkSummaryOptions.FromConfiguration(builder.Configuration));
        builder.Services.AddSingleton<LiveVoteState>();
        builder.Services.AddSingleton<LiveRaceState>();
        builder.Services.AddSingleton<AudienceVoteClient>();
        builder.Services.AddSingleton<QrCodeRenderer>();
        builder.Services.AddSingleton<PresenterNotesWindow>();
        builder.Services.AddTransient<PresenterNotesPage>();
        builder.Services.AddSingleton(_ =>
        {
            using var stream = typeof(MauiProgram).Assembly.GetManifestResourceStream("ChooseYourOwnAdventure.TalkGuide.md")
                ?? throw new InvalidOperationException("The embedded talk guide is missing.");
            using var reader = new StreamReader(stream);
            return new PresenterNotesGuide(reader.ReadToEnd());
        });
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddChooseYourOwnAdventurePresentation();

        return builder.Build();
    }
}
