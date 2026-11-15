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
