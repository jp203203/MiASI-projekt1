using Godot;
using System;
using System.Threading.Tasks;

public partial class GridMover : CharacterBody2D
{
    [Export]
    protected int MoveSpeed = 150;
    [Export]
    protected TileMapLayer GroundLayer;
    [Export]
    protected TileMapLayer RockLayer;
    [Export]
    protected TileMapLayer FoliageLayer;
    [Export]
    protected TileMapLayer WallsLayer;
    [Export]
    protected TileMapLayer BoundaryLayer;

    protected Vector2 TargetPosition;
    protected bool IsMoving = false;

    public override void _Ready()
    {
        TargetPosition = Position;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsMoving)
        {
            Position = Position.MoveToward(TargetPosition, (float)(MoveSpeed * delta));
        }
    }

    protected async Task MoveToWorldPosition(Vector2 nextPosition)
    {
        IsMoving = true;
        TargetPosition = nextPosition;
        await WaitUntilReachedTarget();
        IsMoving = false;
    }

    private async Task WaitUntilReachedTarget()
    {
        while (!Position.IsEqualApprox(TargetPosition))
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
    }

    // check if object collides with the next tile in one of the layers
    protected bool IsTileBlocked(Vector2I tile)
    {
        TileMapLayer[] layers = [GroundLayer, RockLayer, FoliageLayer, WallsLayer, BoundaryLayer];

        foreach (TileMapLayer layer in layers)
        {
            // extract tile data and check for a collision shape
            if (layer != null)
            {
                TileData tileData = layer.GetCellTileData(tile);
                if (tileData != null && tileData.GetCollisionPolygonsCount(0) > 0)
                {
                    return true;
                }
            }

        }

        return false;
    }
}
