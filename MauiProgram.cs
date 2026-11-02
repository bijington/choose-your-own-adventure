using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Scenes;
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
            .UseOrbitGameController(o => { })
            .UseOrbitKeyboard();

        builder.Services.AddSingleton<AdventureState>();
        builder.Services.AddSingleton<AdventureInputRouter>();
        builder.Services.AddSingleton(GameControllerManager.Current);
        builder.Services.AddSingleton(KeyboardManager.Current);
        builder.Services.AddSingleton<OrbitInputBridge>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<OpeningScene>();
        builder.Services.AddTransient<GameLoopScene>();
        builder.Services.AddTransient<VehicleControlScene>();

        return builder.Build();
    }
}
