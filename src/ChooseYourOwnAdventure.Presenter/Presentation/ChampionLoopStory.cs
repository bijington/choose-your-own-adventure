namespace ChooseYourOwnAdventure.Presentation;

public sealed record ChampionLoopBeat(string Thought, string Explanation, string Takeaway);

public static class ChampionLoopStory
{
    public static ChampionLoopBeat For(Champion? champion, LoopStation station)
    {
        var thought = (champion, station) switch
        {
            (Champion.Charioteer, LoopStation.Input) => "A little encouragement for the horses?",
            (Champion.Charioteer, LoopStation.Update) => "The reins are loose. I'm still rolling.",
            (Champion.Charioteer, LoopStation.Render) => "Wheels. Horses. Heroic expression.",
            (Champion.Charioteer, _) => "Another frame. Still no brakes?",
            (Champion.SpacePilot, LoopStation.Input) => "Thrusters, please. Preferably now.",
            (Champion.SpacePilot, LoopStation.Update) => "No thrust? Gravity has opinions.",
            (Champion.SpacePilot, LoopStation.Render) => "Let them see where I've ended up.",
            (Champion.SpacePilot, _) => "One frame closer to a safe landing.",
            (_, LoopStation.Input) => "Foot down. Eyes on the next corner.",
            (_, LoopStation.Update) => "Speed changes. The next corner gets closer.",
            (_, LoopStation.Render) => "Make me look as fast as I feel.",
            _ => "Again. The race won't drive itself."
        };
        return station switch
        {
            LoopStation.Input => new(thought,
                "Ask what the player wants to do.\n\nFor now, imagine the intent. We will wire up real controls in Chapter Two.",
                "Input supplies intent."),
            LoopStation.Update => new(thought,
                champion switch
                {
                    Champion.Charioteer => "Advance position using speed and elapsed time.\n\nMomentum keeps the chariot moving even when no new input arrives.",
                    Champion.SpacePilot => "Apply thrust or gravity, then advance position using elapsed time.\n\nThe world keeps changing when input stops.",
                    _ => "Change speed and advance position using elapsed time.\n\nMovement continues between button presses."
                }, "Update evolves state."),
            LoopStation.Render => new(thought,
                "Draw the champion using the current state.\n\nThis is the Render code we just met: it shows the world that Update produced.",
                "Render makes state visible."),
            _ => new(thought,
                "The engine yields until the next frame, then repeats.\n\nThis is a teaching step-through; the real loop runs continuously.",
                "One frame. Then another.")
        };
    }
}
