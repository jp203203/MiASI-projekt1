using Godot;
using System;
using System.Runtime.InteropServices.JavaScript;

public partial class World : Node2D
{
    public PlayerCharacter PlayerCharacter;
    public TileMapLayer BoundaryLayer;

    private Clock _clock = Clock.Instance;

    // level data ref here?

    public override void _Ready()
    {
        PlayerCharacter = GetNode<PlayerCharacter>("PlayerCharacter");
        BoundaryLayer = GetNode<TileMapLayer>("TileLayerContainer/BoundaryLayer");

        // load level data here probably

        _clock.Pause();
    }

    public void StartCodeExecution()
    {
        _clock.Resume();
    }

    public void ResetWorld()
    {
        PlayerCharacter.ResetToInitial();

        _clock.Reset();
        _clock.Pause();
    }

    public bool IsLevelComplete()
    {
        // logic here when level data (with goal position) is added

        return false;
    }
}
