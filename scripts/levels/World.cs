using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class World : Node2D
{
    [Export] private LevelData _levelData;

    public PlayerCharacter PlayerCharacter;
    public TileMapLayer BoundaryLayer;
    public WallShooterManager WallShooterManager;
	public ItemManager ItemManager;
	public Label LevelCompleted;

    private Clock _clock = Clock.Instance;
    private List<Node> _activeArrows = new();
    

    public override void _Ready()
    {
        PlayerCharacter = GetNode<PlayerCharacter>("PlayerCharacter");
        BoundaryLayer = GetNode<TileMapLayer>("TileLayerContainer/BoundaryLayer");
        WallShooterManager = GetNode<WallShooterManager>("WallShooterManager");
		ItemManager = GetNode<ItemManager>("ItemManager");
		LevelCompleted = GetNode<Label>("CanvasLayer/LevelCompleted");
		
		LevelCompleted.Hide();

        if (_levelData != null) 
            PassShooterData();
        else 
            GD.PushWarning("No level data assigned!");

        WallShooterManager.ArrowSpawned += OnArrowSpawned;

        _clock.Pause();
    }

    private void PassShooterData()
    {
        Dictionary shooterData = _levelData.GetShooterData();
        WallShooterManager.ShooterData = shooterData;
        WallShooterManager.ScanShooters();  // start scanning
    }

    private void OnArrowSpawned(Node arrow)
    {
        // add arrow
        _activeArrows.Add(arrow);
        // remove arrow from the list if it's deleted
        arrow.TreeExited += () => _activeArrows.Remove(arrow);
    }

    public void StartCodeExecution()
    {
        _clock.Resume();
    }

    public void ResetWorld()
    {
        // delete all arrows and clear the list
        foreach (Node arrow in _activeArrows)
        {
            if (IsInstanceValid(arrow))
            {
                arrow.QueueFree();
            }
        }
        _activeArrows.Clear();

        // reset player character position and shooter states
        PlayerCharacter.ResetToInitial();
        WallShooterManager.ResetShooters();
		ItemManager.ResetItems();

        _clock.Reset();
        _clock.Pause();
    }

    public Rect2 GetWorldBounds()
    {
        if (BoundaryLayer == null)
        {
            GD.PushWarning("Boundary layer not found!");
            return new Rect2();
        }

        Rect2I usedRect = BoundaryLayer.GetUsedRect();
        if (usedRect.Size == Vector2.Zero)
        {
            GD.PushWarning("Boundary layer has no tiles!");
            return new Rect2();
        }

        Vector2 tileSize = new Vector2(32, 32);

        // convert tile coords to world coords
        Vector2 topLeft = BoundaryLayer.MapToLocal(usedRect.Position);
        Vector2 bottomRight = BoundaryLayer.MapToLocal(usedRect.Position + usedRect.Size);

        // return boundary rectangle
        return new Rect2(topLeft, bottomRight - topLeft);
    }

    public bool IsLevelComplete()
    {
        // check the tile of the player character
        Vector2 playerTile = BoundaryLayer.LocalToMap(PlayerCharacter.Position);

        // compare the player tile to the goal position
        return (playerTile == _levelData.GoalPosition);
    }
}
