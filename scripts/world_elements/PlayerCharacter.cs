using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class PlayerCharacter : GridMover
{
    [Export]
    bool CanMove = true;
    [Export]
    PackedScene ShieldScene;

    Vector2 PendingDirection = Vector2.Zero;
    string PendingAction = "";
    bool HasShield = false;
    Node2D ShieldNode = null;
    Vector2 InitialPosition;
    Vector2I InitialTile;

    // special tile movement rules
    static readonly Dictionary<string, Dictionary<string, Vector2[]>> TileRules = new()
    {
        {
            "stairs", new()
            {
                { "to", [Vector2.Up, Vector2.Down] },
                { "from", [Vector2.Up, Vector2.Down] },
            }
        },
        {
            "stairs_sideways", new()
            {
                { "to", [Vector2.Right, Vector2.Left] },
                { "from", [Vector2.Right, Vector2.Left] },
            }
        },
        {
            "cliff", new()
            {
                { "to", [Vector2.Up, Vector2.Right, Vector2.Left] },
                { "from", [Vector2.Down, Vector2.Right, Vector2.Left] },
            }
        }
    };

    public override void _Ready()
    {
        base._Ready();

        // store initial state for reset
        InitialPosition = Position;
        InitialTile = GroundLayer.LocalToMap(Position);

        // connect to turn clock
    }

    private void OnTurnTick(int turnIndex)
    {
        if (!CanMove) return;

        // 1st priority - take action
        if (PendingAction != "")
        {
            ExecutePendingAction();
            PendingAction = "";
            return;
        }

        // 2nd priority - move in pending direction
        if (PendingDirection != Vector2.Zero)
        {
            ExecutePendingMove();
            PendingDirection = Vector2.Zero;
        }
    }

    private void ExecutePendingAction()
    {
        switch (PendingAction)
        {
            case "shield":
                ActivateShield();
                break;
            default:
                GD.PushWarning("Unknown action: ", PendingAction);
                break;

        }
    }

    private async Task ExecutePendingMove()
    {
        Vector2I currentTile = GroundLayer.LocalToMap(Position);
        Vector2I nextTile = currentTile + (Vector2I)(PendingDirection);

        if (CanMoveFrom(currentTile, PendingDirection) && CanMoveTo(nextTile, PendingDirection))
        {
            await MoveToTile(nextTile);
        }
    }

    private async Task MoveToTile(Vector2I nextTile)
    {
        CanMove = false;
        Vector2 nextPos = GroundLayer.MapToLocal(nextTile);
        await MoveToWorldPosition(nextPos);
        CanMove = true;
    }

    private bool CanMoveFrom(Vector2I tile, Vector2 direction)
    {
        TileData tileData = RockLayer.GetCellTileData(tile);
        if (tileData == null) return true;

        var tileType = (string)tileData.GetCustomData("type");
        if (TileRules.ContainsKey(tileType))
        {
            return TileRules[tileType]["from"].Contains(direction);
        }

        return true;
    }

    private bool CanMoveTo(Vector2I tile, Vector2 direction)
    {
        // check collision first
        if (IsTileBlocked(tile)) return false;

        // check special rules
        TileData tileData = RockLayer.GetCellTileData(tile);
        if (tileData != null)
        {
            string tileType = (string)tileData.GetCustomData("type");
            if (TileRules.ContainsKey(tileType))
            {
                return TileRules[tileType]["to"].Contains(direction);
            }
        }

        return true;
    }

    private void ActivateShield()
    {
        if (HasShield) return;  // cannot activate shield if it's already present

        // add shield to the scene as a player character child
        ShieldNode = (Node2D)ShieldScene.Instantiate();
        ShieldNode.Modulate = new Color(1, 1, 1, 0.5f);
        ShieldNode.ZIndex = 3;
        AddChild(ShieldNode);

        HasShield = true;
    }

    public void BreakShield()
    {
        if (ShieldNode != null)
        {
            ShieldNode.QueueFree();
            ShieldNode = null;
        }

        HasShield = false;
    }

    public void ResetToInitial()
    {
        Position = InitialPosition;
        TargetPosition = InitialPosition;
        CanMove = true;
        PendingDirection = Vector2.Zero;
        PendingAction = "";
        IsMoving = false;

        if (HasShield) BreakShield();
    }
}
