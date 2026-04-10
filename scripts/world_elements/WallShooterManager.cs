using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class WallShooterManager : Node
{
	[Export]
	private TileMapLayer _groundLayer;
	[Export]
	private TileMapLayer _rockLayer;
	[Export]
	private TileMapLayer _foliageLayer;
	[Export]
	private TileMapLayer _wallsLayer;
	[Export]
	private TileMapLayer _boundaryLayer;
	[Export]
	private PlayerCharacter _playerCharacter;
	[Export]
	private PackedScene _arrowScene;
	[Export]
	public Dictionary ShooterData;

	[Signal]
	public delegate void ArrowSpawnedEventHandler(Node arrow);

	private List<Dictionary> _shooters = new();
	private List<Dictionary> _initialShooterStates = new();

	private Clock _clock = Clock.Instance;

	public override void _Ready()
	{
		_clock.Tick += OnTurnTick;
	}

	private void OnTurnTick(int currentCycle)
	{
		foreach (Dictionary shooter in _shooters)
		{
			int counter = shooter["counter"].AsInt32() + 1;
			int interval = shooter["interval"].AsInt32();

			shooter["counter"] = Variant.From(counter);  // why is this so counterintuitive 

			if (counter % interval == 0)
			{
				FireArrow(shooter);
				shooter["counter"] = Variant.From(0);
			}
		}
	}

	// spawn an arrow node at the shooter tile
	private void FireArrow(Dictionary shooter)
	{
		Arrow arrow = _arrowScene.Instantiate<Arrow>();

		Vector2 tileLocalPos = _wallsLayer.MapToLocal(shooter["tile_pos"].AsVector2I());
		Vector2 tileGlobalPos = _wallsLayer.ToGlobal(tileLocalPos);

		var worldNode = GetParent<Node2D>();
		arrow.Position = worldNode.ToLocal(tileGlobalPos);

		arrow.Direction = shooter["direction"].AsVector2();
		arrow.GroundLayer = _groundLayer;
		arrow.RockLayer = _rockLayer;
		arrow.FoliageLayer = _foliageLayer;
		arrow.WallsLayer = _wallsLayer;
		arrow.BoundaryLayer = _boundaryLayer;
		arrow.PlayerCharacter = _playerCharacter;

		GetParent().AddChild(arrow);
		EmitSignal(SignalName.ArrowSpawned, arrow);
	}

	public void ScanShooters()
	{
		Rect2I usedRect = _wallsLayer.GetUsedRect();

		for (int x = usedRect.Position.X; x < usedRect.Position.X + usedRect.Size.X; x++)
		{
			for (int y = usedRect.Position.Y; y < usedRect.Position.Y + usedRect.Size.Y; y++)
			{
                TileData tileData = _wallsLayer.GetCellTileData(new Vector2I(x, y));
				if (tileData != null && tileData.HasCustomData("is_shooter") && tileData.GetCustomData("is_shooter").AsBool())
				{
					Vector2I key = new Vector2I(x, y);

					// default values if there is no data for shooter in dict
					int interval = ShooterData.ContainsKey(key) ? (int)ShooterData[key].AsGodotDictionary()["interval"] : 5;
					int counter = ShooterData.ContainsKey(key) ? (int)ShooterData[key].AsGodotDictionary()["counter"] : 0;

					string dirStr = tileData.GetCustomData("direction").AsString();
					Vector2 dirVec = dirStr switch
					{
						"down" => Vector2.Down,
						"right" => Vector2.Right,
						"left" => Vector2.Left,
						_ => Vector2.Zero
					};

					_shooters.Add(new Dictionary
					{
						{ "tile_pos", Variant.From(new Vector2I(x, y)) },
						{ "direction", Variant.From(dirVec) },
						{ "interval", Variant.From(interval) },
						{ "counter", Variant.From(counter) }
					});
				}
			}
		}

		_initialShooterStates = new List<Dictionary>(_shooters);
	}

	public void ResetShooters()
	{
		_shooters = new List<Dictionary>(_initialShooterStates);
	}
}
