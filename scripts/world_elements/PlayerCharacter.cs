using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class PlayerCharacter : GridMover
{
    [Export] bool CanMove = true;
    [Export] PackedScene ShieldScene;
    [Export] public ItemManager ItemManager;


    private Clock _clock = Clock.Instance;

    private static readonly Vector2[] Directions = { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right };
    private int _directionIdx = 0;
    Vector2 ActiveDirection;

    string PendingAction = "";
    public bool HasShield = false;
    Node2D ShieldNode = null;
    Vector2 InitialPosition;
    Vector2I InitialTile;

    private bool _itemTaken = false;

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

        ActiveDirection = Directions[_directionIdx];

        // connect to turn clock
        _clock.Tick += OnTurnTick;
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
        if (ActiveDirection != Vector2.Zero)
        {
            ExecutePendingMove();
        }
    }

    private void ExecutePendingAction()
    {
        switch (PendingAction)
        {
            case "shield":
                ActivateShield();
                break;
            case "take":
                TakeItem();
                break;
            case "drop":
                DropItem();
                break;
            default:
                GD.PushWarning("Unknown action: ", PendingAction);
                break;

        }
    }

    private async Task ExecutePendingMove()
    {
        Vector2I currentTile = GroundLayer.LocalToMap(Position);
        Vector2I nextTile = currentTile + (Vector2I)(ActiveDirection);

        if (CanMoveFrom(currentTile, ActiveDirection) && CanMoveTo(nextTile, ActiveDirection))
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
        if (IsTileBlocked(tile, false)) return false;

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

    private void TakeItem()
    {
        if (_itemTaken) return;  // player can't pick up more than one item at once


        Vector2I itemTile = ItemLayer.LocalToMap(Position) + (Vector2I)ActiveDirection;

        if (IsTileBlocked(itemTile, true)) return;  // if there is anything else blocking the way - return (item shouldn't be there)

        TileData item = ItemLayer.GetCellTileData(itemTile);

        if (item != null && (bool)item.GetCustomData("IsItem"))
        {
            ItemManager.RemoveItem(itemTile);
            _itemTaken = true;
        }
    }
    
    private void DropItem()
    {
        if (!_itemTaken) return;  // player can't drop an item if they're not holding one

        Vector2I itemTile = ItemLayer.LocalToMap(Position) + (Vector2I)ActiveDirection;

        if (IsTileBlocked(itemTile, true)) return;  // can't drop an item if there's something blocking the way

        TileData item = ItemLayer.GetCellTileData(itemTile);

        // condition: if there's no item in that space or if the item quantity is less than 3
        if (item == null || ((bool)item.GetCustomData("IsItem") && (int)item.GetCustomData("ItemQuantity") < 3))
        {
            ItemManager.PlaceItem(itemTile);
            _itemTaken = false;
        }
    }

    public void ResetToInitial()
    {
        Position = InitialPosition;
        TargetPosition = InitialPosition;
        CanMove = true;
        ActiveDirection = Directions[0];
        PendingAction = "";
        IsMoving = false;

        if (HasShield) BreakShield();
    }

    public void Rotate(string direction)
    {
        if (direction != "left" || direction != "right")
            GD.PushError("Incorrect rotation direction");

        switch (direction)
        {
            case "right":
                _directionIdx = (_directionIdx + 1) % 4;
                break;
            case "left":
                _directionIdx = (_directionIdx - 1 + 4) % 4;
                break;
        }

        ActiveDirection = Directions[_directionIdx];
    }
}
