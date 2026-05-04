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

    private Queue<PlayerCommand> commandQueue = new Queue<PlayerCommand>();
    
    private Clock _clock = Clock.Instance;

    private static readonly Vector2[] Directions = { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right };
    private int _directionIdx = 0;
    Vector2 ActiveDirection;

	private PlayerCommand _currentCommand = null;
	private int _remainingMoveSteps = 0;
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

        Position = GroundLayer.MapToLocal(GroundLayer.LocalToMap(Position));
    	TargetPosition = Position;
		
		// store initial state for reset
        InitialPosition = Position;
        InitialTile = GroundLayer.LocalToMap(Position);

        ActiveDirection = Directions[_directionIdx];

        // connect to turn clock
        _clock.Tick += OnTurnTick;
    }

    private async void OnTurnTick(int turnIndex)
	{
	    if (!CanMove || IsMoving)
	        return;

	    if (_currentCommand == null && commandQueue.Count == 0)
	    {
	        _clock.Reset();
        	_clock.Pause();
	        return;
	    }
		
	    if (_currentCommand == null)
	    {
	        if (commandQueue.Count == 0)
	            return;

	        _currentCommand = commandQueue.Dequeue();

	        if (_currentCommand.Type == PlayerCommandType.Move)
	            _remainingMoveSteps = _currentCommand.IntParam;
	    }

	    while (true)
		{
		    if (_currentCommand == null)
		    {
		        if (commandQueue.Count == 0)
		            return;

		        _currentCommand = commandQueue.Dequeue();

		        if (_currentCommand.Type == PlayerCommandType.Move)
		            _remainingMoveSteps = _currentCommand.IntParam;
		    }

		    bool consumedTick = await ExecuteCurrentCommandStep();

		    if (consumedTick)
		        break;
		}
	}
	
	private async Task<bool> ExecuteCurrentCommandStep()
	{
	    switch (_currentCommand.Type)
	    {
	        case PlayerCommandType.Move:
	            await ExecuteMoveStep();
	            return true;

	        case PlayerCommandType.Rotate:
	            Rotate(_currentCommand.StringParam);
	            FinishCommand();
	            return true;

	        case PlayerCommandType.Take:
	            TakeItem();
	            FinishCommand();
	            return true;

	        case PlayerCommandType.Drop:
	            DropItem();
	            FinishCommand();
	            return true;

	        case PlayerCommandType.Shield:
	            ActivateShield();
	            FinishCommand();
	            return true;

			case PlayerCommandType.If:
			{
			    bool result = _currentCommand.Condition();

			    List<PlayerCommand> chosen = result
			        ? _currentCommand.ThenBody
			        : _currentCommand.ElseBody;

			    if (chosen != null && chosen.Count > 0)
			    {
			        commandQueue = new Queue<PlayerCommand>(
			            chosen.Concat(commandQueue)
			        );
			    }

			    FinishCommand();
			    return false;
			}
			
			case PlayerCommandType.While:
			    if (_currentCommand.Condition())
			    {
			        var newQueue = new Queue<PlayerCommand>(
			            _currentCommand.Body
			                .Concat(new[] { _currentCommand })
			                .Concat(commandQueue)
			        );

			        commandQueue = newQueue;
			    }

			    FinishCommand();
			    return false;
	    }

	    return true;
	}
	
	private async Task ExecuteMoveStep()
	{
	    if (_remainingMoveSteps <= 0)
	    {
	        FinishCommand();
	        return;
	    }

	    Vector2I currentTile = GroundLayer.LocalToMap(Position);
	    Vector2I nextTile = currentTile + (Vector2I)ActiveDirection;

	    if (!CanMoveFrom(currentTile, ActiveDirection) ||
	        !CanMoveTo(nextTile, ActiveDirection))
	    {
	        FinishCommand();
	        return;
	    }

	    await MoveToTile(nextTile);

	    _remainingMoveSteps--;

	    if (_remainingMoveSteps <= 0)
	        FinishCommand();
	}
	
	private void FinishCommand()
	{
	    _currentCommand = null;
	    _remainingMoveSteps = 0;
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
	
	private Vector2 DirectionFromString(string direction)
	{
	    switch (direction)
	    {
	        case "UP": return Vector2.Up;
	        case "DOWN": return Vector2.Down;
	        case "LEFT": return Vector2.Left;
	        case "RIGHT": return Vector2.Right;
	        default:
	            GD.PushError($"Unknown direction: {direction}");
	            return Vector2.Zero;
	    }
	}
	
	public bool HasItemInDirection(string direction)
	{
	    Vector2 dir = DirectionFromString(direction);
	    Vector2I tile = ItemLayer.LocalToMap(Position) + (Vector2I)dir;

	    TileData item = ItemLayer.GetCellTileData(tile);

	    return item != null && (bool)item.GetCustomData("IsItem");
	}

	public bool HasObstacleInDirection(string direction)
	{
	    Vector2 dir = DirectionFromString(direction);
	    Vector2I tile = GroundLayer.LocalToMap(Position) + (Vector2I)dir;

	    return IsTileBlocked(tile, false);
	}

    private void ActivateShield()
    {
        if (HasShield) return;  // cannot activate shield if it's already present

        // add shield to the scene as a player character child
        ShieldNode = (Node2D)ShieldScene.Instantiate();
        ShieldNode.Modulate = new Color(1, 1, 1, 0.5f);
        ShieldNode.ZIndex = 3;
		ShieldNode.Position = GetNode<CollisionShape2D>("CollisionShape2D").GetPosition();
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

        if (IsTileBlocked(itemTile)) return;  // if there is anything else blocking the way - return (item shouldn't be there)

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

        if (IsTileBlocked(itemTile)) return;  // can't drop an item if there's something blocking the way

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
        Position = GroundLayer.MapToLocal(InitialTile);
    	TargetPosition = Position;
        CanMove = true;
        _directionIdx = 0;
   		ActiveDirection = Directions[_directionIdx];
		_currentCommand = null;
		_remainingMoveSteps = 0;
        IsMoving = false;
		_itemTaken = false;

        if (HasShield) BreakShield();
    }

    public void Rotate(string direction)
    {
        if (direction != "LEFT" && direction != "RIGHT")
            GD.PushError("Incorrect rotation direction");

        switch (direction)
        {
            case "RIGHT":
                _directionIdx = (_directionIdx + 1) % 4;
                break;
            case "LEFT":
                _directionIdx = (_directionIdx - 1 + 4) % 4;
                break;
        }

        ActiveDirection = Directions[_directionIdx];
    }
    
    public void DebugPrintQueue()
    {
        GD.Print("[DEBUG] Current command queue:");
        
        int i = 0;
        foreach (var cmd in commandQueue)
        {
            GD.Print($"[{++i}] {cmd.Type} | Int: {cmd.IntParam} | Str: {cmd.StringParam}");
        }
    }
    
    public void EnqueueCommand(PlayerCommand command)
    {
        commandQueue.Enqueue(command);
    }
	
	public void ClearQueue()
	{
		commandQueue.Clear();
		_currentCommand = null;
		_remainingMoveSteps = 0;
		IsMoving = false;
	}
	
	public Queue<PlayerCommand> CaptureQueue()
	{
	    return new Queue<PlayerCommand>(commandQueue);
	}

	public void RestoreQueue(Queue<PlayerCommand> savedQueue)
	{
	    commandQueue = new Queue<PlayerCommand>(savedQueue);
	}
	
	public List<PlayerCommand> DrainQueueToList()
	{
	    var list = new List<PlayerCommand>();

	    while (commandQueue.Count > 0)
	    {
	        list.Add(commandQueue.Dequeue());
	    }

	    return list;
	}
}
