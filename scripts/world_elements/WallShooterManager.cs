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
    private Dictionary _shooterData;

    [Signal]
    public delegate void ArrowSpawnedEventHandler(Node arrow);

    private List<Dictionary> _shooters = new();
    private List<Dictionary> _initialShooterStates = new();

    private Clock _clock = Clock.Instance;

    public override void _Ready()
    {
        _clock.Tick += OnTurnTick; 
    }
}
