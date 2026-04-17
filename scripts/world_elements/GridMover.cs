using Godot;
using System;
using System.Threading.Tasks;

public partial class GridMover : CharacterBody2D
{
    [Export]
    protected int MoveSpeed = 150;
    [Export]
    public TileMapLayer GroundLayer { get; set; }
    [Export]
    public TileMapLayer RockLayer;
    [Export]
    public TileMapLayer FoliageLayer;
    [Export]
    public TileMapLayer WallsLayer;
    [Export]
    public TileMapLayer BoundaryLayer;
    [Export]
    public TileMapLayer ItemLayer;

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
    protected bool IsTileBlocked(Vector2I tile, bool checkForItem = false)
    {
        TileMapLayer[] layers = [GroundLayer, RockLayer, FoliageLayer, WallsLayer, BoundaryLayer, ItemLayer];

        int layersToCheck = checkForItem ? 6 : 5;

        for (int i = 0; i < layersToCheck; i++)
        {
            // extract tile data and check for a collision shape
            if (layers[i] != null)
            {
                TileData tileData = layers[i].GetCellTileData(tile);
                if (tileData != null && tileData.GetCollisionPolygonsCount(0) > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
