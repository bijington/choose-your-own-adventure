using ChooseYourOwnAdventure.Scenes;
using Orbit.Engine;

namespace ChooseYourOwnAdventure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseOrbitEngine();

        builder.Services.AddSingleton<AdventureState>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<OpeningScene>();

        return builder.Build();
    }
}
