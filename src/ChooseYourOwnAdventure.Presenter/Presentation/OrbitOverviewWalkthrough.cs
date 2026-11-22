namespace ChooseYourOwnAdventure.Presentation;

public sealed record OrbitOverviewStep(
    string Component, string Title, string Explanation, string Takeaway, string TalkingPoints);

public static class OrbitOverviewWalkthrough
{
    public static OrbitOverviewStep ForStage(OrbitOverviewStage stage) => stage switch
    {
        OrbitOverviewStage.Maui => new(
            ".NET MAUI", "Start with a MAUI app",
            "Windows, pages and controls.\n\nA home for the game view.\n\nMenus and presenter notes can remain ordinary MAUI UI.",
            "MAUI hosts the experience.",
            "Start with the question: how could we make a game in MAUI? We already have an application, windows, pages and controls. We need somewhere to display a world that changes over time.\nThis presentation is itself a MAUI app. The audience book contains a game view; the presenter-notes window uses ordinary XAML controls. We can combine both approaches in one application."),
        OrbitOverviewStage.Graphics => new(
            "MAUI GRAPHICS", "Give the world a canvas",
            "Draw shapes, text and images with ICanvas.\n\nTurn the current game state into pixels.\n\nDrawing is one part of making a game.",
            "Graphics supplies the drawing tools.",
            "MAUI Graphics gives us ICanvas: shapes, text, images and transforms. We could build a game directly on these drawing tools.\nWe would still need to decide how to organize the world, advance state over time and schedule updates and drawing. That is the opening for Orbit. The later Render examples use these same drawing APIs."),
        OrbitOverviewStage.Orbit => new(
            "ORBIT", "Add structure and a loop",
            "Scenes organize the world.\n\nGameObjects give its parts behaviour.\n\nUpdate evolves state. Render draws it.",
            "Orbit gives the game a structure.",
            "Reveal the logo here: this is where Orbit enters the picture. Orbit is the lightweight game engine between our drawing tools and our game. GameSceneView hosts it in MAUI; scenes and GameObjects organize it.\nThe engine runs the update/render loop. We put our state changes in Update and drawing in Render. MAUI Graphics stays underneath. We will add the separate Orbit.Input package when we reach the controls chapter."),
        OrbitOverviewStage.Game => new(
            "YOUR GAME", "Write the rules of the world",
            "Choose the characters and their state.\n\nDecide how input changes that state.\n\nMake every change visible to the player.",
            "Your code makes it your game.",
            "Our code defines the experience: which champion appears, how a car steers, how a chariot keeps momentum or how gravity affects a ship.\nMAUI hosts it, Graphics supplies drawing, Orbit supplies the game structure and loop, and our code supplies the rules. The game remains a MAUI application.\nNow show how little setup it takes to add Orbit to that app."),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
}
