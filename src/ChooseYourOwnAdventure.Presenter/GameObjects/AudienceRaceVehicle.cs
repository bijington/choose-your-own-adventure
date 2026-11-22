using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class AudienceRaceVehicle : ChampionVehicleGameObject
{
    private readonly Champion _champion;
    private readonly LiveRaceState _raceState;
    private readonly float _laneOffset;
    private double _displayProgress;
    private Sprite? _motion;
    private bool _isMoving;

    public AudienceRaceVehicle(Champion champion, LiveRaceState raceState, float laneOffset)
    {
        _champion = champion;
        _raceState = raceState;
        _laneOffset = laneOffset;
    }

    public Champion Champion => _champion;
    public double DisplayProgress => _displayProgress;

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);

        var snapshot = _raceState.Snapshot;
        var target = _champion switch
        {
            Champion.Charioteer => snapshot.CharioteerProgress,
            Champion.SpacePilot => snapshot.SpacePilotProgress,
            _ => snapshot.RacingDriverProgress
        };
        if (snapshot.Phase == RacePhase.Ready)
        {
            _displayProgress = 0;
            _isMoving = false;
            return;
        }
        _isMoving = snapshot.Phase == RacePhase.Racing && target - _displayProgress > .0002d;
        if (_isMoving)
        {
            _motion ??= ChampionSprites.CreateMotion(_champion);
            _motion.Update(millisecondsSinceLastUpdate);
        }
        var elapsed = millisecondsSinceLastUpdate / 1000d;
        var smoothing = Math.Clamp(elapsed * 7d, 0d, 1d);
        _displayProgress += (target - _displayProgress) * smoothing;

        if (Math.Abs(target - _displayProgress) < 0.0002d)
        {
            _displayProgress = target;
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        var track = AudienceRaceLayout.GetTrackBounds(dimensions);
        var visualProgress = (_displayProgress + 0.018d) % 1d;
        var angle = visualProgress * Math.PI * 2d - Math.PI / 2d;
        var rx = track.Width * 0.5f + _laneOffset;
        var ry = track.Height * 0.5f + _laneOffset * 0.6f;
        var centreX = track.X + track.Width / 2f;
        var centreY = track.Y + track.Height / 2f;

        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        var x = centreX + cos * rx;
        var y = centreY + sin * ry;
        var dx = -rx * sin;
        var dy = ry * cos;
        var heading = (float)(Math.Atan2(dy, dx) * 180d / Math.PI);

        canvas.SaveState();
        canvas.Translate(x, y);
        canvas.Rotate(heading);

        if (_isMoving && _motion is not null)
        {
            // Motion frames face up, while the track heading uses right as zero.
            canvas.Rotate(90f);
            _motion.Bounds = new RectF(-64, -80, 128, 160);
            _motion.Render(canvas, _motion.Bounds);
        }
        else
        {
            DrawVehicle(canvas, _champion);
        }

        canvas.RestoreState();
    }


}

public static class AudienceRaceLayout
{
    public static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.58f;
        var height = dimensions.Height * 0.48f;
        return new RectF(
            dimensions.X + dimensions.Width * 0.30f,
            dimensions.Y + dimensions.Height * 0.23f,
            width,
            height);
    }
}
