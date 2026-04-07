using Godot;
using Godot.NativeInterop;
using System;
using System.Threading.Tasks;

public partial class Arrow : GridMover
{
    [Signal]
    public delegate void PlayerHitWithoutShieldEventHandler();

    [Export]
    private Vector2 _direction = Vector2.Zero;
    [Export]
    private PlayerCharacter _playerCharacter;

    private Area2D _collisionArea;
    private Sprite2D _sprite;
    private Clock _clock = Clock.Instance;

    private Vector2I _startTile;

    public override void _Ready()
    {
        base._Ready();

        _collisionArea = GetNode<Area2D>("CollisionArea");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _clock = GetNode<Clock>("root/Clock");

        _startTile = GroundLayer.LocalToMap(Position);

        // rotate the texture to match the movement direction
        RotateSpriteForDirection();

        // connect signals
        _clock.Tick += OnTurnClock;
        _collisionArea.BodyEntered += OnBodyEntered;
    }

    private void RotateSpriteForDirection()
    {
        if (_direction == Vector2.Left)
            _sprite.Rotation = float.Pi * 0.5f;
        else if (_direction == Vector2.Right)
            _sprite.Rotation = -float.Pi * 0.5f;
    }

    private void OnTurnClock(int cycle)
    {
        if (!IsMoving) MoveOneTile();
    }

    private async Task MoveOneTile()
    {
        Vector2I currentTile = GroundLayer.LocalToMap(Position);
        Vector2I nextTile = currentTile + (Vector2I)_direction;

        // remove the arrow on collision
        if (IsTileBlocked(nextTile))
        {
            QueueFree();
            return;
        }

        // movement to the next tile
        Vector2 nextPosition = GroundLayer.MapToLocal(nextTile);
        await MoveToWorldPosition(nextPosition);
    }

    private void OnBodyEntered(Node body)
    {
        // only player collision matters
        if (body != _playerCharacter) return;

        // check for shield
        if (_playerCharacter.HasShield)
            _playerCharacter.BreakShield();
        else
            EmitSignal(SignalName.PlayerHitWithoutShield);

        // delete arrow after hit
        QueueFree();
    }
}
