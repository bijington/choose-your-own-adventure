# Choose Your Own Adventure — read-aloud rehearsal

Updated 22 November 2026. Matches the current presentation deck and its local reveals. Aim for **30–35 minutes including decisions, demonstrations and audience interaction**. The text is a speaking guide, not a promise of exact runtime: time your first pass and let the demonstrations breathe.

Read the ordinary paragraphs aloud. **Bracketed cues are actions, not spoken words.** Use slide titles rather than page numbers; champion branches change the printed numbers. The old verification log is preserved in [REHEARSAL-HISTORY.md](REHEARSAL-HISTORY.md). Technical detail and fallback procedures remain in [TALK-GUIDE.md](TALK-GUIDE.md).

## Before starting — do not read aloud

- For a solo run, enable **Practice mode** through the mobile options button or **Presenter → Practice Options**. Choose a champion on the voting spread; skip the race when you reach it. Read the race instructions aloud anyway. Use the Driver script on the first pass, then rehearse the other movement variants separately.
- For a live test, choose **New Audience Session** before anybody joins. Select **Use live audience** if practice mode is enabled. The session and QR link change together; everybody must scan the new code. A new session is remembered on this device across restarts.
- Put the audience window on the presentation display and the notes/script on your own display. Confirm keyboard/controller input and QR readability.
- **Continue** below means South / Enter or a right-page tap, unless a cue says otherwise. Right shoulder / Page Down advances ordinary slides and staged explanations; on the game-loop walkthrough it leaves the slide. Left shoulder / Page Up navigates back.
- Wait for the page turn to finish before pressing again. Read only the applicable champion or fallback paragraph.
- Existing audio under `rehearsal/audio` predates this script. It has not been regenerated and will not follow all the current slides.

## Opening — cover

Hello, I’m Shaun Lawrence, and this is Choose Your Own Adventure.

Do you remember those books where you put a finger between the pages because you weren’t completely convinced that opening the mysterious door was a good idea?

Today, we’re bringing a little of that to a .NET MAUI application. You’ll choose our champion, shape the example we explore, and eventually become the input that powers the final challenge.

We’ll start with ordinary MAUI, give our champion a world and some movement, and then connect this screen to your phones. The technology won’t just be something we discuss. It will be running the session.

**[Continue → Your First Decision.]**

## Your First Decision — the after-party choice

Before we do anything technical, you have an important decision to make.

We can begin our adventure… or, given where I am in the schedule, we could try going straight to the after-party.

**[Pause for a show of hands or a shouted answer. This is not a SignalR vote.]**

**[If taking the joke branch, select the after-party card → page 404.]**

After-party not found. Apparently, we have to complete the adventure first. I did try.

**[Continue from page 404, or select the main adventure card → Discovering Orbit.]**

## Prologue — Discovering Orbit

Every adventure needs a world to happen in. Ours happens inside a MAUI app.

That’s worth keeping in mind throughout the talk. We aren’t leaving our application behind. We’re adding another way to make part of it feel alive.

**[Continue → MAUI Already Has Some Magic.]**

## MAUI Already Has Some Magic — live animations

Before we reach for anything else, MAUI already gives us a little magic.

These are ordinary XAML controls. We can move them, rotate them, scale them and change their opacity. Easing changes the feel of that movement: the same journey can feel mechanical, gentle or a little more dramatic.

**[Point to the bounce, rotating star and heartbeat. Let them play. Tap Play it again if useful.]**

The code on the left animates properties over time. The star doesn’t need a complete game engine just to make an entrance.

This is great for transitions, feedback and playful UI. As we add characters, rules and a changing world, it becomes useful to organize those things around a shared game loop.

Let’s build up the pieces.

**[Tap Build a game →, or continue outside the control panel.]**

## Building a Game in MAUI — four reveals

### 1 / 4 — .NET MAUI

We begin with the application itself: windows, pages, layouts and controls.

MAUI gives our game a home. This presentation is an example: the audience sees a game view inside a page, while the presenter notes use ordinary XAML UI in another window.

We can combine these approaches. We don’t have to choose between an application and a game-like experience.

**[Continue once → MAUI Graphics.]**

### 2 / 4 — MAUI Graphics

Next, we need somewhere to draw our world.

MAUI Graphics gives us a canvas: shapes, text, images and transforms. We could build a game directly using those tools.

But drawing is only one part of it. We also need to organize the world, advance its state over time and arrange when updates and drawing happen.

**[Continue once → Orbit. The logo now appears.]**

### 3 / 4 — Orbit

This is where Orbit comes in.

Orbit is a lightweight game engine built on MAUI Graphics. Scenes organize the world, game objects give its parts behaviour, and the engine runs the update-and-render loop.

We write the changes in Update and show the result in Render. The drawing tools underneath are still MAUI Graphics.

**[Continue once → Your Game.]**

### 4 / 4 — Your Game

Then there’s the part no engine can decide for us: what our game actually does.

Which character appears? How does it respond to input? Does it steer around corners, keep rolling after we let go, or fall because gravity has opinions?

MAUI hosts it. Graphics draws it. Orbit supplies the structure and loop. Our code supplies the rules.

**[Continue → Install & Register Orbit.]**

## Install & Register Orbit

The setup starts in a familiar place: a NuGet package.

Add Bijington.Orbit.Engine, then register it with UseOrbitEngine when building the MAUI application.

We’re still working inside a normal MAUI app. This gives us the engine services we’ll use to host and run a scene.

**[Continue → Give Orbit Somewhere to Render.]**

## Give Orbit Somewhere to Render

Now we put a GameSceneView in our page, just like another MAUI view.

It gives Orbit somewhere to display the current scene. We can place it alongside ordinary UI, fill a page with it, or build a more elaborate presentation around it.

The view is the window into the game world. We still need to decide what world it should show.

**[Continue → Load a Game Scene.]**

## Load a Game Scene

We ask the scene manager to load a scene into that view, then start the loop.

Loading chooses the world. Starting begins the repeating update-and-render work that keeps it alive.

That is the basic route: register the engine, give it a view, load a scene and start.

Our scene is ready. What it needs now is a main character.

**[Continue → Meet Our Champion.]**

## Chapter One — Meet Our Champion

A little self-discovery. A few lines of C#. Let’s find out what our champion is made of.

But first, you get to decide who we meet.

**[Continue → Choose Your Champion.]**

## Choose Your Champion — live vote

Scan the QR code and choose your champion.

We have the Driver: speed, precision and questionable braking. The Charioteer: glory, courage and momentum. Or the Space Pilot: exploration, thrusters and, naturally, Orbit.

This choice changes the movement example we’ll explore. Your own vote also chooses your team for the final race, even if another champion wins this opening round.

**[Live: press Continue to open voting. Give people time to scan and vote. In practice: tap the desired champion card to reveal a local winner, then use the practice cue below.]**

I’ll give you a moment. The way those votes reach this screen is part of the story. We’ll come back to it later.

**[Live: Continue closes voting. Wait at least three seconds, then Continue reveals the result. If tied, use the tie paragraph below. After the winner reveal has been visible for at least two seconds, Continue accepts the champion.]**

**[Practice: after the local reveal has been visible for at least two seconds, Continue accepts the champion. Do not read the live closing instructions aloud.]**

### Only if the live vote is tied

Democracy has produced an interesting plot twist. We have a tie. Let’s try one more round.

**[Continue reopens voting; close, wait and reveal again. Do not try to continue to the summary while there is no winner.]**

## Our Champion — summary and consequence

You’ve chosen more than our hero — you’ve chosen the physics we’ll have to live with.

**[Read one variant.]**

**Driver:** You chose precision. We’ll accelerate, steer and find out why knowing when to brake is part of going quickly.

**Charioteer:** You chose momentum. A little encouragement gets us moving; letting go doesn’t immediately make that movement disappear.

**Space Pilot:** You chose gravity. We can apply thrust to rise, but releasing the control doesn’t make the universe stop updating.

Before we deal with those consequences, let’s explore our champion’s backstory. Every champion has an origin story. Ours begins with a dot.

**[Continue → Dot on the same spread.]**

## Our Champion’s Origins — four reveals

### 1 / 4 — Dot

Here are our humble beginnings: a filled ellipse.

We have a canvas, some bounds, a colour and a drawing instruction. It isn’t much of a hero yet, but we can already put something into the world.

**[Continue → Primitives.]**

### 2 / 4 — Primitives

Combine a few circles, rectangles and lines, and we have something recognizable.

The API hasn’t changed. We’re just giving it more instructions. We can build quite a lot this way, but more handmade visual detail means more drawing code.

**[Continue → Image.]**

### 3 / 4 — Image

Now the artwork carries that detail for us.

We load and cache an image, preserve its proportions and draw it into the available space. Our champion looks much richer, while the code that puts them on screen is quite small.

This is still one static picture.

**[Continue → Animated Sprite.]**

### 4 / 4 — Animated Sprite

And now our champion has a little more life.

This uses Orbit’s Sprite with a sequence of celebration frames. We create it once, advance the animation in Update using elapsed time, and draw the current frame in Render.

A sprite doesn’t have to be animated, but this one is. The artwork has evolved from a dot into a moving character, while the canvas-and-bounds rendering contract remains familiar.

There’s one question hiding in that explanation: who keeps calling Update and Render?

**[Continue → A Familiar Rhythm, the firmware comparison.]**

## A Familiar Rhythm — firmware first

If you’ve worked with firmware, this pattern may look familiar.

Read inputs. Update state. Write outputs. Wait, then repeat.

The game loop has a similar rhythm. We collect intent, evolve the world, render what it looks like now, and yield until the next frame.

The snippets are a conceptual comparison. We don’t need to write our own blocking while loop in a MAUI page; Orbit runs the loop for us.

Let’s follow one frame in our champion’s life.

**[Continue once → the four-station loop walkthrough.]**

## A Frame in the Life of Our Champion — four stations

### Input

First, input: what does the player want to do?

Accelerate, pull the reins, or apply thrust. At this point we’re describing intent, not drawing anything and not directly moving the character.

**[Continue → Update.]**

### Update

Then Update changes the state using that intent and the elapsed time.

**[Read one variant.]**

**Driver:** Speed changes, and our position advances. The next corner gets closer between button presses.

**Charioteer:** Position advances using the current speed. Momentum keeps the chariot rolling even when no new input arrives.

**Space Pilot:** Thrust or gravity changes velocity, and velocity changes position. When the input stops, the world keeps changing.

That’s a useful distinction: no new event has happened, but the world is still alive.

**[Continue → Render.]**

### Render

Render makes that state visible. It draws the world Update has produced.

This is the drawing contract we just explored. It isn’t responsible for deciding how far the character should move; it shows where the character is now.

**[Continue → Wait.]**

### Wait

The engine yields until the next frame, then repeats.

We’re stepping through this slowly to explain it. In the real game, it runs continuously.

Input affects state. Update evolves state. Render displays state. One frame, then another.

**[Use Right shoulder / Page Down or the lower-right TAKE CONTROL button to leave this slide. Continue alone cycles through the stations.]**

## Chapter Two — Take Control

Our champion has a world, a look and a rhythm. Now let’s give them something to respond to.

**[Continue → MAUI Already Has Input.]**

## MAUI Already Has Input

Just as MAUI already gives us animation, it already gives us useful input.

A Button works with touch or a mouse. Clicked or a command can represent an action; Pressed and Released are useful when the action needs to stay active while the control is held.

In this example, pressing sets an intent flag and releasing clears it. The handler doesn’t move or redraw the champion. Update and Render still do that work.

Gestures and pointer events give us other options. For a touch-first game-like feature, ordinary MAUI UI may be exactly what we need.

What if we also want a keyboard or a game controller?

**[Continue → How Does It Move?]**

## How Does It Move? — Orbit.Input

Orbit.Input is a separate package for keyboard and game-controller input.

We register the input support we want, then interpret controls as actions in our application.

The particular button is less important than the intent it represents. South might mean accelerate here; the same action could come from a keyboard or a touch control.

Now let’s see the consequence of your champion choice.

**[Continue → Drive Our Champion.]**

## Drive Our Champion — live demonstration

**[Read and perform only the selected variant. Spend roughly one minute demonstrating, rather than reading all three.]**

### Driver

This is the challenge you chose for us: precision.

**[Hold South / Enter to accelerate. Use the left stick X axis or Left/Right arrows to steer. Use East / Shift / Down to brake.]**

Acceleration changes speed. Steering changes our position across the track. Braking reduces the speed again.

Watch the state and the highlighted code as I change the input. The tyre animation makes the movement easier to read, but the important part is underneath: input changes state, then the next render shows it.

### Charioteer

This is the challenge you chose for us: momentum.

**[Hold South / Enter to pull, release, then use East / Shift / Down to brake.]**

A pull builds speed. When I release the control, the chariot keeps rolling. We don’t need another input event for that to happen; Update keeps advancing the position from the existing velocity.

The wheels and dust help us see it, but the behaviour comes from the state model. A release is not the same thing as a stop.

### Space Pilot

This is the challenge you chose for us: gravity.

**[Hold South / Enter to thrust, release to show falling, then use East / Shift / Down for down-thrust if useful.]**

Thrust changes velocity. Release, and gravity keeps acting on the next update. The exhaust appears while we’re applying thrust, but our position continues changing after that input ends.

We don’t tell the ship a final destination. We change the forces affecting it and let the loop evolve the result.

### Rejoin the main script

Three different movement models, but the same overall pattern: intent, state changes and rendering.

So far, that intent has come from a local control. What if we changed where it came from?

**[Up / W or the lower-right NEXT area → An Adventure Together. On touch, tapping the main demo pulses the action; use the explicit NEXT area to leave.]**

## Chapter Three — An Adventure Together

One controller is useful. An entire room is more interesting.

Let’s connect this world to all of you.

**[Continue → What If the Input Source Changes?]**

## What If the Input Source Changes?

What if, instead of this controller, the input came from all of you?

The game still needs intent. The source of that intent changes, but the downstream state and rendering model can stay familiar.

And the interesting thing is: you have already been using the architecture that makes this possible. You used it when you voted for our champion.

Let’s follow one of those votes from your phone to this screen.

**[Continue → One Vote, Four Steps.]**

## One Vote, Four Steps — first explain SignalR

SignalR is ASP.NET Core’s library for real-time communication between a server and connected clients.

Normally, a browser asks for something and the server replies. With SignalR, the server can also push an update when something changes, without every phone repeatedly asking whether there is news.

Think of the hub as the room’s switchboard. Your phone sends a choice. Our application records it and announces the updated state to the connected clients in this session.

The phones and this MAUI app are clients of the same server. SignalR carries the messages; our application decides what they mean.

### 1 / 4 — Audience browser

Here is the JavaScript on the phone.

It builds a connection to adventureHub, starts it and joins our session. When you choose a champion, it invokes Vote with the session, your participant identity and your choice.

The phone sends “I choose this champion.” It doesn’t calculate the winner or send a new total for everybody else to trust.

**[Continue → Hub.]**

### 2 / 4 — ASP.NET Core hub

On the server, MapHub connects that URL to AdventureHub.

The Vote invocation reaches the public Vote method, with those arguments in the same order. The actual implementation also normalizes and validates the incoming values; the slide keeps the entry point short enough to follow.

Joining a session puts the connection into its session group. That lets us send updates to the people in this adventure.

**[Continue → State and broadcast.]**

### 3 / 4 — Server state and broadcast

The vote store owns the shared state. While voting is open, it records one choice per participant and returns a snapshot. A lock protects those updates when people act at the same time.

Then the hub broadcasts VoteUpdated to the session group, including the phones and this presentation.

The hub routes the message. The store owns the state. The server pushes the result, rather than waiting for the presentation to ask again.

**[Continue → MAUI client and Orbit.]**

### 4 / 4 — MAUI client and Orbit

On this side, the MAUI client listens for VoteUpdated. Notice that the event name matches the broadcast.

The callback updates our local copy of the state. OpeningScene reads that state during the ordinary Orbit loop and draws the totals and the reveal.

SignalR doesn’t draw the scene and doesn’t need to start another game loop. It changes the state the existing loop sees.

Browser sends intent. Server owns shared state. Orbit renders the result.

Our final challenge follows that same route: Boost intent goes in, RaceUpdated state comes out. The browser collects pending taps and sends their count every hundred milliseconds, rather than requiring a separate call for every tap. At ordinary tapping speeds, some batches will still contain only one tap.

**[Continue → Audience Grand Prix.]**

## Audience Grand Prix — the final challenge

This is our final challenge. You chose your champion earlier, and you have been on that team ever since. Now power them through the last stretch.

Before I start, there’s a design bug we should think about.

What happens if one champion has twice as many people? Have we built a race — or a headcount?

**[Pause for an answer. The race stays Ready while you explain.]**

Raw tap totals would reward the biggest team. Our server divides tap strength by team size, so progress depends on average effort per registered teammate. A small party can still win this adventure.

People who voted but aren’t tapping still count, so encourage your teammates.

**[Live: confirm everybody has their team’s boost control. Continue starts the race. Practice: do not expect a live race; read the countdown, pause to imagine the interaction, then use Continue/right page to skip to the epilogue.]**

Ready? Three, two, one — go!

**[Let the audience race. The target is roughly 28 seconds at three taps per second per person, not a fixed timer. Faster teams can finish sooner. Avoid explaining code during the action.]**

**[Optional short commentary; choose one, don’t read the list.]**

Driver team are awake.

Space Pilot appears to have discovered horizontal scaling.

Charioteers, I admire your commitment to eventual consistency.

**[After Finished, name the winning team. In practice, don’t invent a result; say the solo-run paragraph below instead.]**

The phone didn’t move that vehicle directly. It sent intent, the server changed the authoritative race state, and Orbit rendered the result.

You chose the heroes, shaped their journey and powered the final challenge. The room wrote the ending.

**[Live: Continue after Finished → The Journey Home.]**

### Solo-run paragraph only

This is where I’ll celebrate the winning team, then connect the result back to the same intent, state and rendering path. For this rehearsal, I’m skipping the live race and continuing to the ending.

**[Practice: Continue/right page → The Journey Home. A new practice session has no race winner to celebrate.]**

## Epilogue — The Journey Home

Every race has a finish. Every adventure leaves us with something to take home.

**[Continue → The Podium.]**

## The Podium

**[Live: acknowledge the actual winner and the other teams. Pause for reaction. Solo: skip the celebration if there is no result.]**

Thank you for playing. The champion vote, the movement demo and that race all came back to the same idea: give intent a route into shared state, and make the result visible.

**[Continue → What Would We Add Next?]**

## What Would We Add Next? — feedback ideas

We’ve just made a room full of phones into controllers. If we built the next version, I’d start with a question: when I tap Boost, how do I know the game felt it?

On a controller I can feel the button move. On glass, a short haptic pulse could give me a little of that feeling back on a device that supports it. I’d make it optional, especially when we’re asking people to tap several times a second.

Sound can help with a different moment. Imagine hearing the last beep of the countdown while you’re looking at your champion on the big screen. Or give the finish its own sound so the whole room knows the race is over. I’d play that one when the server confirms the finish, so it matches the result we’re showing.

And if I’ve muted my phone or turned vibration off, the buttons and the race still need to make sense. A quick highlight on Boost and a visible countdown can carry the same information.

**[Allow roughly 60–90 seconds. Pause after the opening question if you invite an answer.]**

**[Continue → Adventure Complete.]**

## Adventure Complete — summary and resources

We started with MAUI: an application, ordinary controls, input and animation.

We added Orbit to organize a world with scenes and game objects, and keep it alive through Update and Render.

Then we used SignalR to let the whole room become an input source. The phones sent intent, the server owned shared state, and our MAUI app rendered the result.

You don’t need to be building a full game to use these ideas. A responsive interaction, a playful visualization or a shared live experience can make an application feel much more alive.

**[If the resources URL is configured and the QR code is visible, read this paragraph.]**

The QR code links to the companion blog post, with resources from the talk. Take a photo or scan it before you head out.

**[If it is not configured, omit that paragraph. Populate TalkSummary:ResourcesUrl before the real talk; don’t direct people to a missing link.]**

I’m Shaun Lawrence. Thank you for choosing this adventure with me.

And yes, AI was used in the making of these slides — but no agents were harmed in the process.

**[Pause. Leave the final spread visible for questions and the QR code.]**

## Emergency cards — read only if needed

### Setup or connectivity trouble

There were going to be goblins. This one appears to live in the conference Wi-Fi.

**[Optional joke only; there is no planned goblin reveal. Allow reconnect/rejoin to recover the latest snapshot. Keep the same browser identity to retain the team. If necessary, enable practice mode, choose locally and skip the race. Don’t promise replay of every missed tap.]**

The network is one possible source of input. We can keep exploring the game locally while it sorts itself out.

### Running short

**[Keep all four building blocks, but use one sentence per reveal. Shorten the movement demonstration and the four SignalR steps. Preserve the vote, final challenge when networking permits, and closing summary. Skip optional jokes and repeated demos.]**

The detail to remember is the same at every stage: input affects state, Update evolves it, and Render makes it visible.
