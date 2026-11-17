# Local rehearsal — 15 November 2026

## Verified

- Solution restore and Mac Catalyst presenter build succeeded. Final presenter and web builds report zero warnings and zero errors.
- Cascadia font is present and renders as a monospaced font in the Apple graphics canvas.
- Walked the 17-scene presentation using touch navigation, including voting, loop/firmware beats, control demo, SignalR explanation, audience race, podium and recap.
- Observed Driver and Charioteer branch instructions and following footers: 28 and 58. Observed Space Pilot's following footer at 88; its instruction uses the same page helper as the other branches.
- Touch acceleration, chariot momentum and space-pilot thrust/gravity changed the demo state.
- Corrected clipped headings, the branch instruction, code samples and podium labels. Confirmed the final podium places first in the centre, second left and third right using shared vehicle GameObjects.
- State checks passed with 600 concurrent participants: votes retained, ties preserved, team-normalized race progress, closed team selection and invalid tap rejection.
- Live SignalR checks passed with three independent clients: presenter authorization, invalid champion rejection, tie/reopen/reveal, original team retention, reconnect, race finish, join URL and local JavaScript asset.
- Added a regression for 20+4 tap batches from two teammates: floating-point roundoff must not leave a full progress bar racing indefinitely.

Run the repeatable checks using [these instructions](tests/Adventure.Rehearsal/README.md).

## Still needs target hardware / deployment

- Keyboard forward/back navigation and simultaneous controller throttle/steering/braking. Synthesized Return presses through desktop automation did not advance the app; the walkthrough used touch. Verify a real keyboard and controller before presenting.
- Physical phones, QR scanning at projector distance, all interactive highlights while holding physical inputs, and actual projector dimensions.
- Azure-hosted SignalR, realistic concurrent network connections, slow/mobile networks and reconnects. The 600-participant check exercises the in-process store, not network capacity.
- Deployment URL and matching presenter key. No Azure resource was selected or deployed in this rehearsal. Keep the local defaults until the deployment destination is known.

Existing Mac Catalyst project settings and asynchronous startup ordering were preserved. No commits were created; pre-existing uncommitted work remains in the checkout.

## Open-book prototype follow-up

The presenter now hosts its content in one `BookScene` with an outgoing bitmap for page turns. The Mac Catalyst build passes with zero warnings/errors. Desktop checks confirmed forward turns, reverse turns using the lower-left corner, facing-page code and illustrations, offline champion branching, loop-marker updates, and touch thrust/gravity after navigation. Existing vote/race state checks still pass. The app was left open on the Orbit overview spread for review.

The turn is a stylised paper fold with the incoming page printed on its reverse, rather than a curved 3D page simulation. Book layouts and transitions have been checked on Mac Catalyst; Android/iOS and projector dimensions still need rehearsal.


The transition outline has been removed, and the old stationary page remains under the incoming leaf to avoid duplicated new text. The turn clock starts after image capture. Champion selection now places instructions/QR/status on the left and three destination-labelled cards on the right. Desktop checks confirmed the revised card layout and that tapping the middle card selects the Charioteer/page-58 branch.

The standalone branch-result scene has been removed. The selection spread fades away losing cards and grows the winner to fill the right page, then continues directly to Add GameObject. Existing destination pages are preserved by retiring page 7. Desktop checks confirmed an offline Charioteer reveal, direct continuation to page 58, returning to the final selection result, and left-page back navigation from the voting spread to Load a Game Scene. Escape now goes back from voting; B/Shift resets the round. Final Mac Catalyst build has zero warnings/errors, and the vote/race rehearsal checks pass.


Added a presenter-controlled first decision after the cover: page 2 begins the talk, while page 404 delivers the after-party-not-found joke and rejoins page 2. No SignalR vote is opened for this introduction. Existing content now carries prologue/chapter/epilogue running headers. Mac Catalyst builds cleanly; desktop checks confirmed the two choice routes, page-404 footer, prologue header and Back routing to the choice. Existing vote/race checks pass. Champion destinations stay 28/58/88.


Fixed the vote-to-code flash: OpeningScene now renders only voting content and no longer switches into legacy character/code-reveal modes when the chosen champion changes shared state. After accepting the result, a dedicated right-page ChampionChapterScene shows CHAPTER ONE / MEET YOUR CHAMPION, the selected name and the shared ChampionGameObject on bare parchment. The left leaf stays blank. Chapter destinations are 27/57/87; Add GameObject remains 28/58/88. Desktop checks confirmed Space Pilot confirmation → chapter 87 → code 88, clean outgoing captures and back navigation. Left the app on the chapter reveal. Build: zero warnings/errors; existing rehearsal checks pass.

Added matching right-page openers for Prologue, Chapter Two, Chapter Three and Epilogue using the shared ChapterOpeningScene layout. Chapter One is now MEET OUR CHAMPION and adds: “A little self-discovery. A few lines of C#. Let’s find out what our champion is made of.” Its introduction and larger artwork fit without overlap. Story page numbers now derive from deck order, excluding the cover and optional page 404; champion offsets still apply from Chapter One. Voting destinations now match chapter 28/58/88, with Add GameObject at 29/59/89.

Desktop checks verified all five opener layouts, the new voting destination, normal chapter progression, driving → Chapter Three, and a local SignalR race finish → Epilogue. The app is left on The Journey Home. The temporary web host was stopped after rehearsal. Mac Catalyst build has zero warnings/errors, the existing rehearsal checks pass, and the talk guide has updated chapter/page order.

Corrected Chapter One’s narrative order: MEET OUR CHAMPION now opens before the vote with its self-discovery text and no selected character. After the final vote, ChosenChampionScene gives a simple right-page “THE AUDIENCE HAS CHOSEN / OUR CHAMPION” summary with the selected name and shared GameObject. It does not repeat a chapter heading. The sequence is chapter → vote → summary → Add GameObject. Offsets begin at the summary; winning cards point to 29/59/89 and code follows at 30/60/90. Verified the chapter-before-vote route and Charioteer summary in the desktop app. Build has zero warnings/errors; existing rehearsal checks pass. Left the app on the chosen-character summary.


Simplified the champion section: the chosen-character summary now keeps the champion visible on the right and reveals Render()/drawing-primitives examples on the left as a local beat. The next action opens Take Control. Left-page tap/Escape hides code before navigating back to voting. Removed the separate AddGameObjectScene and ChampionRevealScene and updated the guide/page map. Mac Catalyst build: zero warnings/errors. Existing rehearsal checks passed. Desktop visual QA could not complete because the computer-use preview connection expired; this version has not been visually verified.
